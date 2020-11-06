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
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Services;
using Xamarin.Essentials.Interfaces;
#if DEBUG
using Android.Util;
#endif

#pragma warning disable CS0618 // Type or member is obsolete
namespace Rocks.Wasabee.Mobile.Droid.Infra.Firebase
{
    [Service()]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class WasabeeFcmService : FirebaseMessagingService
    {
        const string TAG = "[WASABEE_FCM_SERVICE]";

        private IMvxMessenger _mvxMessenger;
        private ILoginProvider _loginProvider;
        private IBackgroundDataUpdaterService _backgroundDataUpdaterService;
        private ISecureStorage _secureStorage;

        private int _lastId = 0;
        private MvxSubscriptionToken _mvxToken;

        private string _fcmToken = string.Empty;
        private bool _isInitialized = false;

        public WasabeeFcmService()
        {

        }

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

            _loginProvider ??= Mvx.IoCProvider.Resolve<ILoginProvider>();
            _mvxMessenger ??= Mvx.IoCProvider.Resolve<IMvxMessenger>();
            _backgroundDataUpdaterService ??= Mvx.IoCProvider.Resolve<IBackgroundDataUpdaterService>();
            _secureStorage ??= Mvx.IoCProvider.Resolve<ISecureStorage>();

            _mvxToken ??= _mvxMessenger.Subscribe<UserLoggedInMessage>(async msg => await SendRegistrationToServer());

            _isInitialized = true;

            if (FirebaseInstanceId.Instance.Token != null)
            {
                var instanceIdToken = FirebaseInstanceId.Instance.Token;

                if (!instanceIdToken.Equals(_fcmToken))
                {
                    if (_fcmToken.IsNullOrEmpty())
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
                        await _backgroundDataUpdaterService.UpdateMarker(opId.Value, markerId.Value).ConfigureAwait(false);
                }
                else if (messageBody.Contains("Link"))
                {
                    var opId = message.Data.FirstOrDefault(x => x.Key.Equals("opID"));
                    var linkId = message.Data.FirstOrDefault(x => x.Key.Equals("linkID"));

                    if (!string.IsNullOrWhiteSpace(opId.Value) && !string.IsNullOrWhiteSpace(linkId.Value))
                        await _backgroundDataUpdaterService.UpdateLink(opId.Value, linkId.Value).ConfigureAwait(false);
                }
                else if (messageBody.Contains("Map Change"))
                {
                    var opId = message.Data.FirstOrDefault(x => x.Key.Equals("opID"));
                    if (!string.IsNullOrWhiteSpace(opId.Value))
                        await _backgroundDataUpdaterService.UpdateOperation(opId.Value).ConfigureAwait(false);
                }
            }
        }

        private void SendNotification(string messageBody)
        {
            var channels = new[]
            {
                "Quit", "Generic Message", "Agent Location Change", "Map Change", "Marker Status Change",
                "Marker Assignment Change", "Link Status Change", "Link Assignment Change", "Subscribe", "Login", "Undefined"
            };

            var intent = new Intent(this, typeof(AndroidSplashScreenActivity));
            intent.AddFlags(ActivityFlags.BroughtToFront);
            intent.PutExtra("FCMMessage", messageBody);

            var channel = "Undefined";
            if (channels.Any(x => x.Equals(messageBody)))
            {
                channel = channels.First(x => x.Equals(messageBody));
            }

            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(this, AndroidMainActivity.CHANNEL_ID);

            notificationBuilder.SetContentTitle("Wasabee")
                .SetSmallIcon(Resource.Drawable.wasabee)
                .SetContentText(messageBody)
                .SetAutoCancel(true)
                .SetShowWhen(false)
                .SetContentIntent(pendingIntent)
                .SetChannelId(channel);

            var notificationManager = NotificationManager.FromContext(this);
            notificationManager?.Notify(_lastId, notificationBuilder.Build());
            _lastId++;
        }
    }
}
#pragma warning restore CS0618