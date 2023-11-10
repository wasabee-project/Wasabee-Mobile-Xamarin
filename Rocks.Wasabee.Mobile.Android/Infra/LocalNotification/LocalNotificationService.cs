using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Rocks.Wasabee.Mobile.Core.Infra.LocalNotification;
using Xamarin.Forms.Platform.Android;

namespace Rocks.Wasabee.Mobile.Droid.Infra.LocalNotification
{
    public class LocalNotificationService : ILocalNotificationService
    {
        private static Context Context => Application.Context;

        private int _lastId;

        public void Send(string messageBody)
        {
            var intent = new Intent(Context, typeof(AndroidMainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            intent.SetPackage(null);

            var pendingIntent = PendingIntent.GetActivity(Context, 0, intent, PendingIntentFlags.OneShot | PendingIntentFlags.Immutable);
            var notificationBuilder = new NotificationCompat.Builder(Context, "Wasabee_Notifications")
                .SetSmallIcon(Resource.Drawable.wasabee)
                .SetContentTitle("Wasabee")
                .SetContentText(messageBody)
                .SetContentIntent(pendingIntent)
                .SetAutoCancel(true)
                .SetDefaults((int) NotificationDefaults.All)
                .SetPriority((int) NotificationPriority.Max)
                .SetColor(Xamarin.Forms.Color.FromHex("#3BA345").ToAndroid().ToArgb());

            var notificationManager = NotificationManager.FromContext(Context);
            notificationManager?.Notify(_lastId, notificationBuilder.Build());
            _lastId++;
        }
    }
}