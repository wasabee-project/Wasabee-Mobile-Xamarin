using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Firebase.Iid;
using Firebase.Messaging;
using MvvmCross;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Cache;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms.Platform.Android;

#if DEBUG
using Android.Util;
#endif

#pragma warning disable CS0618 // Type or member is obsolete
namespace Rocks.Wasabee.Mobile.Droid.Infra.Firebase
{
    [Service]
    [IntentFilter(new[] {"com.google.firebase.MESSAGING_EVENT"})]
    [IntentFilter(new[] {"com.google.firebase.INSTANCE_ID_EVENT"})]
    public class WasabeeFirebaseMessagingService : FirebaseMessagingService
    {
        private const string Tag = "WASABEE_FCM_SERVICE";

        private IMvxMessenger _mvxMessenger;
        private ILoginProvider _loginProvider;
        private IBackgroundDataUpdaterService _backgroundDataUpdaterService;
        private ISecureStorage _secureStorage;
        private IUserSettingsService _userSettingsService;
        private OperationsDatabase _operationsDatabase;

        private MvxSubscriptionToken _mvxToken;

        private int _lastId;
        private string _fcmToken = string.Empty;
        private bool _isInitialized;

        public override async void OnNewToken(string token)
        {
            _fcmToken = token;

            if (!_isInitialized)
                await Initialize();
#if DEBUG
            Log.Debug(Tag, "Token=" + token);
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
            Log.Debug(Tag, message.ToString());
#endif
            if (message.GetNotification() != null)
            {
                //These is how most messages will be received
#if DEBUG
                Log.Debug(Tag, message.GetNotification().Body);
#endif
                SendNotification(message.GetNotification().Body);
            }
            else
            {
                var (_, cmd) = message.Data.FirstOrDefault(x => x.Key.Equals("cmd"));
                var (_, msg) = message.Data.FirstOrDefault(x => x.Key.Equals("msg"));

                var messageBody = $"{cmd} : {msg}";

#if DEBUG
                Log.Debug(Tag, messageBody);
#endif
                _mvxMessenger.Publish(new NotificationMessage(this, messageBody, message.Data));

                var (_, opId) = message.Data.FirstOrDefault(x => x.Key.Equals("opID"));
                var (_, updateId) = message.Data.FirstOrDefault(x => x.Key.Equals("updateID"));

                if (messageBody.Contains("Agent Location Change"))
                {
                    var gid = message.Data.FirstOrDefault(x => x.Key.Equals("gid"));
                    _mvxMessenger.Publish(new TeamAgentLocationUpdatedMessage(this, gid.Value, msg));
                }
                else if (messageBody.Contains("Marker"))
                {
                    var (_, markerId) = message.Data.FirstOrDefault(x => x.Key.Equals("markerID"));

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
                                SendNotification($"{op.Name} : Marker {msg}");
                        }
                    }
                }
                else if (messageBody.Contains("Link"))
                {
                    var (_, linkId) = message.Data.FirstOrDefault(x => x.Key.Equals("linkID"));

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
                                SendNotification($"{op.Name} : Link {msg}");
                        }
                    }
                }
                else if (messageBody.Contains("Map Change"))
                {
                    if (!string.IsNullOrWhiteSpace(opId) && !string.IsNullOrWhiteSpace(updateId))
                    {
                        if (CheckIfUpdateIdShouldBeProcessedOrNot(updateId))
                            await _backgroundDataUpdaterService.UpdateOperationAndNotify(opId).ConfigureAwait(false);
                    }
                }
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
                    Log.Debug(Tag, $"updateID '{updateId}' isn't processed, update will be done");
                }
                else
                {
                    Log.Debug(Tag, $"updateID '{updateId}' has already been processed, update aborted");
#endif
                }
            }
            else
            {
                result = true;
                OperationsUpdatesCache.Data.Add(updateId, true);
#if DEBUG
                Log.Debug(Tag, $"updateID '{updateId}' isn't processed, update will be done");
#endif
            }

            return result;
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _mvxToken?.Dispose();
                _mvxToken = null;
            }
        }
    }
}
#pragma warning restore CS0618