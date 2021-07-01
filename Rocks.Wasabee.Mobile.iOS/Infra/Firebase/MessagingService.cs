using Firebase.CloudMessaging;
using Firebase.InstanceID;
using Foundation;
using MvvmCross;
using MvvmCross.Plugin.Messenger;
using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Infra.Cache;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase.Payloads;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;
using Xamarin.Essentials.Interfaces;
using FirebaseApp = Firebase.Core.App;

namespace Rocks.Wasabee.Mobile.iOS.Infra.Firebase
{
    public class MessagingService : IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        private IMvxMessenger _mvxMessenger;
        private ILoginProvider _loginProvider;
        private IBackgroundDataUpdaterService _backgroundDataUpdaterService;
        private ISecureStorage _secureStorage;
        private IUserSettingsService _userSettingsService;
        private OperationsDatabase _operationsDatabase;

        private MvxSubscriptionToken _mvxToken;
        private string _fcmToken = string.Empty;

        public IntPtr Handle { get; }

        private static MessagingService _instance;
        public static MessagingService Instance => _instance ??= new MessagingService();
        
        private readonly ILoggingService _loggingService;

        public void Dispose()
        {

        }

        public MessagingService()
        {
            _loggingService ??= Mvx.IoCProvider.Resolve<ILoggingService>();
            
            _operationsDatabase ??= Mvx.IoCProvider.Resolve<OperationsDatabase>();

            _loginProvider ??= Mvx.IoCProvider.Resolve<ILoginProvider>();
            _mvxMessenger ??= Mvx.IoCProvider.Resolve<IMvxMessenger>();
            _backgroundDataUpdaterService ??= Mvx.IoCProvider.Resolve<IBackgroundDataUpdaterService>();
            _secureStorage ??= Mvx.IoCProvider.Resolve<ISecureStorage>();
            _userSettingsService ??= Mvx.IoCProvider.Resolve<IUserSettingsService>();
        }

        public async Task Init()
        {
            _mvxToken ??= _mvxMessenger.Subscribe<UserLoggedInMessage>(async msg => await SendRegistrationToServer());

            // Use Firebase library to configure APIs
            FirebaseApp.Configure();

            // Register for remote notifications
            UNUserNotificationCenter.Current.Delegate = this;
            // Register your app for remote notifications.
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // For iOS 10 display notification (sent via APNS)
                UNUserNotificationCenter.Current.Delegate = this;

                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) => {
                    Console.WriteLine(granted);
                });
            }
            else
            {
                // iOS 9 or before
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }

            UIApplication.SharedApplication.RegisterForRemoteNotifications();

            Messaging.SharedInstance.Delegate = this;

            InstanceId.SharedInstance.GetInstanceId(InstanceIdResultHandler);

            if (Messaging.SharedInstance.FcmToken != null)
            {
                var instanceIdToken = Messaging.SharedInstance.FcmToken;
                if (!instanceIdToken.Equals(_fcmToken))
                {
                    if (string.IsNullOrEmpty(_fcmToken))
                        _fcmToken = instanceIdToken;

                    await SendRegistrationToServer();
                }
            }
        }

        private async Task SendRegistrationToServer()
        {
            await _secureStorage.SetAsync(SecureStorageConstants.FcmToken, _fcmToken);
            await _loginProvider.SendFirebaseTokenAsync(_fcmToken);
        }


        private void InstanceIdResultHandler(InstanceIdResult result, NSError error)
        {
            if (error != null) {
                _loggingService.Error($"MessagingService - Error: {error.LocalizedDescription}");
                return;
            }
            
            _loggingService.Trace($"MessagingService - Remote Instance Id token: {result.Token}");
        }

        [Export ("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            // Monitor token generation: To be notified whenever the token is updated.
            _loggingService.Trace("MessagingService - DidReceiveRegistrationToken");

            _fcmToken = fcmToken;

            if (Mvx.IoCProvider.CanResolve<ISecureStorage>())
            {
                Mvx.IoCProvider.Resolve<ISecureStorage>().SetAsync(SecureStorageConstants.FcmToken, _fcmToken);
            }
        }

        public async Task ReceivedMessage(NSDictionary dictionary)
        {
            _loggingService.Trace($"MessagingService - ReceivedMessage => '{dictionary}'");

            var data = dictionary.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
            var (_, cmd) = data.FirstOrDefault(x => x.Key.Equals("cmd"));
            var (_, msg) = data.FirstOrDefault(x => x.Key.Equals("msg"));

            var messageBody = $"{cmd} : {msg}";

            _mvxMessenger.Publish(new NotificationMessage(this, messageBody, data));

            var (_, opId) = data.FirstOrDefault(x => x.Key.Equals("opID"));
            var (_, updateId) = data.FirstOrDefault(x => x.Key.Equals("updateID"));

            if (cmd.Contains("Agent Location Change"))
            {
                var gid = data.FirstOrDefault(x => x.Key.Equals("gid"));
                _mvxMessenger.Publish(new TeamAgentLocationUpdatedMessage(this, gid.Value, msg));
            }
            else if (cmd.Contains("Marker"))
            {
                var (_, markerId) = data.FirstOrDefault(x => x.Key.Equals("markerID"));

                if (!string.IsNullOrWhiteSpace(opId) && !string.IsNullOrWhiteSpace(markerId))
                {
                    if (!string.IsNullOrWhiteSpace(updateId))
                    {
                        if (CheckIfUpdateIdShouldBeProcessedOrNot(updateId))
                            await _backgroundDataUpdaterService.UpdateMarkerAndNotify(opId, markerId)
                                .ConfigureAwait(false);
                    }

                    var op = await _operationsDatabase.GetOperationModel(opId);
                    var marker = op?.Markers.FirstOrDefault(x => x.Id.Equals(markerId));
                    if (marker != null)
                    {
                        var loggedUserGid = _userSettingsService.GetLoggedUserGoogleId();
                        if (marker.AssignedTo.Equals(loggedUserGid))
                            return;//SendNotification($"{op.Name} : Marker {msg}");
                    }
                }
            }
            else if (cmd.Contains("Link"))
            {
                var (_, linkId) = data.FirstOrDefault(x => x.Key.Equals("linkID"));

                if (!string.IsNullOrWhiteSpace(opId) && !string.IsNullOrWhiteSpace(linkId))
                {
                    if (!string.IsNullOrWhiteSpace(updateId))
                    {
                        if (CheckIfUpdateIdShouldBeProcessedOrNot(updateId))
                            await _backgroundDataUpdaterService.UpdateLinkAndNotify(opId, linkId)
                                .ConfigureAwait(false);
                    }

                    var op = await _operationsDatabase.GetOperationModel(opId);
                    var link = op?.Links.FirstOrDefault(x => x.Id.Equals(linkId));
                    if (link != null)
                    {
                        var loggedUserGid = _userSettingsService.GetLoggedUserGoogleId();
                        if (link.AssignedTo.Equals(loggedUserGid))
                            return;//SendNotification($"{op.Name} : Link {msg}");
                    }
                }
            }
            else if (cmd.Contains("Map Change"))
            {
                if (!string.IsNullOrWhiteSpace(opId) && !string.IsNullOrWhiteSpace(updateId))
                {
                    if (CheckIfUpdateIdShouldBeProcessedOrNot(updateId))
                        await _backgroundDataUpdaterService.UpdateOperationAndNotify(opId).ConfigureAwait(false);
                }
            }
            else if (cmd.Equals("Target"))
            {
                var targetPayload = JsonConvert.DeserializeObject<TargetPayload>(msg);
                //SendNotification($"Target from {targetPayload.Sender}: {targetPayload.Name}");
                _mvxMessenger.Publish(new TargetReceivedMessage(this, targetPayload));
            }
        }

        private bool CheckIfUpdateIdShouldBeProcessedOrNot(string updateId)
        {
            var result = false;
            if (OperationsUpdatesCache.Data.ContainsKey(updateId))
            {
                if (OperationsUpdatesCache.Data[updateId] == false)
                {
                    result = true;
                    OperationsUpdatesCache.Data[updateId] = true;
#if DEBUG
                    Console.WriteLine($"updateID '{updateId}' isn't processed, update will be done");
                }
                else
                {
                    Console.WriteLine($"updateID '{updateId}' has already been processed, update aborted");
#endif
                }
            }
            else
            {
                result = true;
                OperationsUpdatesCache.Data.Add(updateId, true);
#if DEBUG
                Console.WriteLine($"updateID '{updateId}' isn't processed, update will be done");
#endif
            }

            return result;
        }
    }
}