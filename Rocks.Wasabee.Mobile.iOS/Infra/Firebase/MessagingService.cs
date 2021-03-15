using System;
using Acr.UserDialogs;
using Firebase.CloudMessaging;
using Foundation;
using MvvmCross;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using UIKit;
using UserNotifications;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.iOS.Infra.Firebase
{
    public class MessagingService : IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        private static MessagingService _instance;
        public static MessagingService Instance => _instance ??= new MessagingService();
        
        public IntPtr Handle { get; }

        public void Dispose()
        {

        }

        public MessagingService()
        {
            
        }

        public void Init()
        {
            //In iOS you must request permission to show local / remote notifications first since it is a user interrupting action.
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // Request Permissions
                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound, 
                    (granted, error) =>
                    {
                        // Do something if needed
                    });
 
                // For iOS 10 display notification (sent via APNS)
                UNUserNotificationCenter.Current.Delegate = this;
 
                // For iOS 10 data message (sent via FCM)
                Messaging.SharedInstance.Delegate = this;
            }
            else
            {
                // iOS 9 or before
                var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
 
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
        }
        
        [Export ("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            Mvx.IoCProvider.Resolve<ILoggingService>().Trace("MessagingService - DidReceiveRegistrationToken");

            if (Mvx.IoCProvider.CanResolve<ISecureStorage>())
            {
                Mvx.IoCProvider.Resolve<ISecureStorage>().SetAsync(SecureStorageConstants.FcmToken, fcmToken);
            }
        }
    }
}