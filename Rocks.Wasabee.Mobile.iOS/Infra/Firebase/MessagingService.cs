using Firebase.CloudMessaging;
using Firebase.InstanceID;
using Foundation;
using MvvmCross;
using Rocks.Wasabee.Mobile.Core.Infra.Firebase;
using Rocks.Wasabee.Mobile.Core.Infra.LocalNotification;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using System;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;
using FirebaseApp = Firebase.Core.App;

namespace Rocks.Wasabee.Mobile.iOS.Infra.Firebase
{
    public class MessagingService : IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        private readonly ILoggingService _loggingService;
        private readonly ICrossFirebaseMessagingService _crossFirebaseMessagingService;
        private readonly ILocalNotificationService _localNotificationService;

        private bool _isInitialized = false;

        private static MessagingService _instance;
        public static MessagingService Instance => _instance ??= new MessagingService();
        
        public IntPtr Handle { get; }

        private MessagingService()
        {
            _loggingService ??= Mvx.IoCProvider.Resolve<ILoggingService>();
            _crossFirebaseMessagingService ??= Mvx.IoCProvider.Resolve<ICrossFirebaseMessagingService>();
            _localNotificationService ??= Mvx.IoCProvider.Resolve<ILocalNotificationService>();

            _crossFirebaseMessagingService.Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;
            try
            {
                // Use Firebase library to configure APIs
                FirebaseApp.Configure();

                // Register for remote notifications
                UNUserNotificationCenter.Current.Delegate = this;
                // Register your app for remote notifications.
                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                {
                    // For iOS 10 display notification (sent via APNS)
                    UNUserNotificationCenter.Current.Delegate = this;

                    var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge |
                                      UNAuthorizationOptions.Sound;
                    UNUserNotificationCenter.Current.RequestAuthorization(authOptions,
                        (granted, error) => { Console.WriteLine(granted); });
                }
                else
                {
                    // iOS 9 or before
                    var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge |
                                               UIUserNotificationType.Sound;
                    var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                    UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
                }

                UIApplication.SharedApplication.RegisterForRemoteNotifications();

                Messaging.SharedInstance.Delegate = this;

                InstanceId.SharedInstance.GetInstanceId(InstanceIdResultHandler);
            }
            catch (Exception e)
            {
                _loggingService.Error(e, "MessagingService - Initialize() failed");
            }
            finally
            {
                _isInitialized = true;
            }
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
        public async void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            // Monitor token generation: To be notified whenever the token is updated.
            _loggingService.Trace("MessagingService - DidReceiveRegistrationToken");
            
            await _crossFirebaseMessagingService.SendRegistrationToServer(fcmToken);
        }

        public async Task ReceivedMessage(NSDictionary nsDictionary)
        {
            _loggingService.Trace($"MessagingService - ReceivedMessage => '{nsDictionary}'");

            var dictionary = nsDictionary.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());

            await _crossFirebaseMessagingService.ProcessMessageData(dictionary);
        }

        public void Dispose()
        {

        }
    }
}