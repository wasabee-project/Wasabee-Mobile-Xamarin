using Firebase.CloudMessaging;
using Foundation;
using MvvmCross.Forms.Platforms.Ios.Core;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Ui;
using System;
using System.Threading.Tasks;
using MvvmCross;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using UIKit;
using UserNotifications;

namespace Rocks.Wasabee.Mobile.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : MvxFormsApplicationDelegate<Setup, CoreApp, App>, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            Rg.Plugins.Popup.Popup.Init();
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            

            //var platformConfig = new PlatformConfig() { ImageFactory = new WasabeeImageFactory() };
            Xamarin.FormsGoogleMaps.Init(MapsKey.Value);
            
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageSourceHandler();

            // Use Firebase library to configure APIs
            /*Firebase.Core.App.Configure();
 
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
 
            UIApplication.SharedApplication.RegisterForRemoteNotifications();*/
 
            return base.FinishedLaunching(app, options);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                Mvx.IoCProvider.Resolve<ILoggingService>().Fatal(exception, "[CurrentDomain] Fatal error occured");
                throw exception;
            }
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Mvx.IoCProvider.Resolve<ILoggingService>().Fatal(e.Exception, "[TaskScheduler] Fatal error occured");
            throw e.Exception;
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (Xamarin.Essentials.Platform.OpenUrl(app, url, options))
                return true;
    
            return base.OpenUrl(app, url, options);
        }

        [Export ("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken (Messaging messaging, string fcmToken)
        {
            Console.WriteLine ($"Firebase registration token: {fcmToken}");
 
            // TODO: If necessary send token to application server.
            // Note: This callback is fired at each app startup and whenever a new token is generated.
        }
        
        //Handle data messages in foregrounded apps
        [Export("messaging:didReceiveMessage:")]
        public void DidReceiveMessage(Messaging messaging, RemoteMessage remoteMessage)
        {
            //Handle here your notification
        }
    }
}
