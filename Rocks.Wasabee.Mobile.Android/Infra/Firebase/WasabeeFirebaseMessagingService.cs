using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Firebase.Messaging;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Messages;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Iid;
using MvvmCross;
using Rocks.Wasabee.Mobile.Core.Infra.Cache;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms.Platform.Android;
#if DEBUG
using Android.Util;
#endif

#pragma warning disable CS0618 // Type or member is obsolete
namespace Rocks.Wasabee.Mobile.Droid.Infra.Firebase
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class WasabeeFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "[WASABEE_FCM_SERVICE]";

        private IMvxMessenger _mvxMessenger;
        private ILoginProvider _loginProvider;
        private IBackgroundDataUpdaterService _backgroundDataUpdaterService;
        private ISecureStorage _secureStorage;
        private IUserSettingsService _userSettingsService;
        private OperationsDatabase _operationsDatabase;

        private int _lastId = 0;
        private MvxSubscriptionToken _mvxToken;

        private string _fcmToken = string.Empty;
        private bool _isInitialized = false;

        public override async void OnNewToken(string token)
        {
            _fcmToken = token;

            if (!_isInitialized)
                await Initialize();
#if DEBUG
            Log.Debug(TAG, "FCM token: " + token);
#endif
        }

        private async Task SendRegistrationToServer()
        {
            if (!_isInitialized)
                await Initialize();

            await _secureStorage.SetAsync(SecureStorageConstants.FcmToken, _fcmToken);
            await _loginProvider.SendFirebaseTokenAsync(_fcmToken);
        }

        private async Task Initialize()
        {
            if (_isInitialized)
                return;

            _operationsDatabase ??= Mvx.IoCProvider.Resolve<OperationsDatabase>();

            _loginProvider ??= Mvx.IoCProvider.Resolve<ILoginProvider>();
            _mvxMessenger ??= Mvx.IoCProvider.Resolve<IMvxMessenger>();
            _backgroundDataUpdaterService ??= Mvx.IoCProvider.Resolve<IBackgroundDataUpdaterService>();
            _secureStorage ??= Mvx.IoCProvider.Resolve<ISecureStorage>();
            _userSettingsService ??= Mvx.IoCProvider.Resolve<IUserSettingsService>();

            _mvxToken ??= _mvxMessenger.Subscribe<UserLoggedInMessage>(async msg => await SendRegistrationToServer());

            _isInitialized = true;

            if (FirebaseInstanceId.Instance.Token != null)
            {
                var instanceIdToken = FirebaseInstanceId.Instance.Token;

                if (!instanceIdToken.Equals(_fcmToken))
                {
                    if (string.IsNullOrEmpty(_fcmToken))
                        _fcmToken = instanceIdToken;

                    await SendRegistrationToServer();
                }
            }
        }

        public override async void OnMessageReceived(RemoteMessage message)
        {
            if (!_isInitialized)
                await Initialize();

#if DEBUG
            Log.Debug(TAG + " : ", message.ToString());
#endif
            if (message.GetNotification() != null)
            {
                //These is how most messages will be received
#if DEBUG
                Log.Debug(TAG + " : ", message.GetNotification().Body);
#endif
                SendNotification(message.GetNotification().Body);
            }
            else
            {
                var cmd = message.Data.FirstOrDefault(x => x.Key.Equals("cmd"));
                var msg = message.Data.FirstOrDefault(x => x.Key.Equals("msg"));

                var messageBody = $"{cmd.Value} : {msg.Value}";

#if DEBUG
                Log.Debug(TAG + " : ", messageBody);
#endif
                _mvxMessenger.Publish(new NotificationMessage(this, messageBody, message.Data));

                if (messageBody.Contains("Agent Location Change"))
                {
                    var gid = message.Data.FirstOrDefault(x => x.Key.Equals("gid"));
                    _mvxMessenger.Publish(new TeamAgentLocationUpdatedMessage(this, gid.Value, msg.Value));
                }
                else if (messageBody.Contains("Marker"))
                {
                    var opId = message.Data.FirstOrDefault(x => x.Key.Equals("opID"));
                    var markerId = message.Data.FirstOrDefault(x => x.Key.Equals("markerID"));

                    if (!string.IsNullOrWhiteSpace(opId.Value) && !string.IsNullOrWhiteSpace(markerId.Value))
                    {
                        await _backgroundDataUpdaterService.UpdateMarkerAndNotify(opId.Value, markerId.Value).ConfigureAwait(false);
                        
                        var op = await _operationsDatabase.GetOperationModel(opId.Value);
                        if (op != null)
                        {
                            var marker = op.Markers.FirstOrDefault(x => x.Id.Equals(markerId.Value));
                            if (marker != null)
                            {
                                var loggedUserGid = _userSettingsService.GetLoggedUserGoogleId();
                                if (marker.AssignedTo.Equals(loggedUserGid))
                                    SendNotification($"{op.Name} : Marker {msg.Value}");
                            }
                        }
                    }
                }
                else if (messageBody.Contains("Link"))
                {
                    var opId = message.Data.FirstOrDefault(x => x.Key.Equals("opID"));
                    var linkId = message.Data.FirstOrDefault(x => x.Key.Equals("linkID"));

                    if (!string.IsNullOrWhiteSpace(opId.Value) && !string.IsNullOrWhiteSpace(linkId.Value))
                    {
                        await _backgroundDataUpdaterService.UpdateLinkAndNotify(opId.Value, linkId.Value).ConfigureAwait(false);
                        
                        var op = await _operationsDatabase.GetOperationModel(opId.Value);
                        if (op != null)
                        {
                            var link = op.Links.FirstOrDefault(x => x.Id.Equals(linkId.Value));
                            if (link != null)
                            {
                                var loggedUserGid = _userSettingsService.GetLoggedUserGoogleId();
                                if (link.AssignedTo.Equals(loggedUserGid))
                                    SendNotification($"{op.Name} : Link {msg.Value}");
                            }
                        }
                    }
                }
                else if (messageBody.Contains("Map Change"))
                {
                    var opId = message.Data.FirstOrDefault(x => x.Key.Equals("opID"));
                    var updateId = message.Data.FirstOrDefault(x => x.Key.Equals("updateID"));

                    var shouldUpdate = false;
                    if (!string.IsNullOrWhiteSpace(opId.Value) && !string.IsNullOrWhiteSpace(updateId.Value))
                    {
                        if (OperationsUpdatesCache.Data.ContainsKey(updateId.Value))
                        {
                            if (OperationsUpdatesCache.Data[updateId.Value] == false)
                            {
                                shouldUpdate = true;
                                OperationsUpdatesCache.Data[updateId.Value] = true;
                            }
                        }
                        else
                        {
                            shouldUpdate = true;
                            OperationsUpdatesCache.Data.Add(updateId.Value, true);
                        }

                        if (shouldUpdate)
                            await _backgroundDataUpdaterService.UpdateOperationAndNotify(opId.Value).ConfigureAwait(false);
                    }
                }
            }
        }

        private void SendNotification(string messageBody)
        {
            var intent = new Intent(this, typeof(AndroidMainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            intent.SetPackage(null);
            
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(this, "Wasabee_Notifications");

            notificationBuilder
                .SetSmallIcon(Resource.Drawable.wasabee)
                .SetContentTitle("Wasabee")
                .SetContentText(messageBody)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetDefaults((int) NotificationDefaults.All)
                .SetPriority((int) NotificationPriority.Max)
                .SetColor(Xamarin.Forms.Color.FromHex("#3BA345").ToAndroid().ToArgb());

            var notificationManager = NotificationManager.FromContext(this);
            notificationManager?.Notify(_lastId, notificationBuilder.Build());
            _lastId++;
        }
    }
}
#pragma warning restore CS0618