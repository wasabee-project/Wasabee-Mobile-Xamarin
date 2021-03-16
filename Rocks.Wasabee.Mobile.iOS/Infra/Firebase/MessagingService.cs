using System;
using Firebase.CloudMessaging;
using Firebase.InstanceID;
using Foundation;
using MvvmCross;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using UIKit;
using UserNotifications;
using Xamarin.Essentials.Interfaces;
using FirebaseApp = Firebase.Core.App;

namespace Rocks.Wasabee.Mobile.iOS.Infra.Firebase
{
    public class MessagingService : IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        public IntPtr Handle { get; }

        private static MessagingService _instance;
        public static MessagingService Instance => _instance ??= new MessagingService();
        
        
        private readonly ILoggingService _loggingService;

        public void Dispose()
        {

        }

        public MessagingService()
        {
            _loggingService = Mvx.IoCProvider.Resolve<ILoggingService>();
        }

        public void Init()
        {
            // Use Firebase library to configure APIs
            FirebaseApp.Configure();

            // Register for remote notifications
            UNUserNotificationCenter.Current.Delegate = this;

            var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
            UNUserNotificationCenter.Current.RequestAuthorization (authOptions, (granted, error) => {
                Console.WriteLine (granted);
            });
            
            UIApplication.SharedApplication.RegisterForRemoteNotifications();

            Messaging.SharedInstance.Delegate = this;

            InstanceId.SharedInstance.GetInstanceId(InstanceIdResultHandler);
 
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

            if (Mvx.IoCProvider.CanResolve<ISecureStorage>())
            {
                Mvx.IoCProvider.Resolve<ISecureStorage>().SetAsync(SecureStorageConstants.FcmToken, fcmToken);
            }
        }

        [Export ("messaging:didReceiveMessage:")]
        public void DidReceiveMessage(Messaging messaging, RemoteMessage remoteMessage)
        {
            // TODO Handle message
            _loggingService.Trace($"MessagingService - DidReceiveMessage => '{remoteMessage.AppData}'");
        }
    }
}