using Android.App;
using Firebase.Messaging;
using MvvmCross;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase;
using Rocks.Wasabee.Mobile.Core.Infra.LocalNotification;

#if DEBUG
using Android.Util;
#endif

#pragma warning disable CS0618 // Type or member is obsolete
namespace Rocks.Wasabee.Mobile.Droid.Infra.Firebase
{
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class WasabeeFirebaseMessagingService : FirebaseMessagingService
    {
        private const string Tag = "WASABEE_FCM_SERVICE";

        private ICrossFirebaseMessagingService _crossFirebaseMessagingService;
        private ILocalNotificationService _localNotificationService;

        private bool _isInitialized;

        public override async void OnNewToken(string token)
        {
            if (!_isInitialized)
                Initialize();

            await _crossFirebaseMessagingService.SendRegistrationToServer(token);
#if DEBUG
            Log.Debug(Tag, "Token=" + token);
#endif
        }

        public override async void OnMessageReceived(RemoteMessage message)
        {
            if (!_isInitialized)
                Initialize();

#if DEBUG
            Log.Debug(Tag, message.Data.ToDebugString());
#endif
            if (message.GetNotification() != null)
            {
                //These is how most messages will be received
#if DEBUG
                Log.Debug(Tag, message.GetNotification().Body);
#endif
                _localNotificationService.Send(message.GetNotification().Body);
            }
            else
            {
                await _crossFirebaseMessagingService.ProcessMessageData(message.Data).ConfigureAwait(false);
            }
        }

        #region Private methods

        private void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            _crossFirebaseMessagingService ??= Mvx.IoCProvider.Resolve<ICrossFirebaseMessagingService>();
            _localNotificationService ??= Mvx.IoCProvider.Resolve<ILocalNotificationService>();

            _crossFirebaseMessagingService.Initialize();
        }

        #endregion
    }
}
#pragma warning restore CS0618 // Type or member is obsolete