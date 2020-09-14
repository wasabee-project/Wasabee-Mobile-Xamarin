using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Firebase.Messaging;
using MvvmCross;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Messages;
using System.Linq;
using System.Threading.Tasks;

#if DEBUG
using Android.Util;
#endif

namespace Rocks.Wasabee.Mobile.Droid.Infra.Firebase
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT", "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class WasabeeFcmService : FirebaseMessagingService
    {
        const string TAG = "[WASABEE_FCM_SERVICE]";

        private readonly IMvxMessenger _mvxMessenger;
        private readonly ILoginProvider _loginProvider;

        private int _lastId = 0;
        private MvxSubscriptionToken _mvxToken;

        private string _fcmToken = string.Empty;

        public WasabeeFcmService()
        {
            _loginProvider = Mvx.IoCProvider.Resolve<ILoginProvider>();
            _mvxMessenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();

            _mvxToken = _mvxMessenger.Subscribe<UserLoggedInMessage>(async msg => await SendRegistrationToServer(_fcmToken));
        }

        public override void OnNewToken(string token)
        {
#if DEBUG
            Log.Debug(TAG, "FCM token: " + token);
#endif
            _fcmToken = token;
        }

        private async Task SendRegistrationToServer(string token)
        {
            await _loginProvider.SendFirebaseTokenAsync(token);
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
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
                var cmd = message.Data.First(x => x.Key.Equals("cmd"));
                var msg = message.Data.First(x => x.Key.Equals("msg"));

                var messageBody = $"{cmd.Value} : {msg.Value}";

#if DEBUG
                Log.Debug(TAG + " : ", messageBody);
#endif
                _mvxMessenger.Publish(new NotificationMessage(this, messageBody));

                if (messageBody.Contains("Agent Location Change"))
                    _mvxMessenger.Publish(new TeamAgentLocationUpdatedMessage(this));
            }
        }

        void SendNotification(string messageBody)
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