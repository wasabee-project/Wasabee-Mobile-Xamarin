using Foundation;
using MvvmCross.Forms.Platforms.Ios.Core;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Ui;
using System;
using System.Threading.Tasks;
using MvvmCross;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using UIKit;
using Xamarin.Forms.GoogleMaps.iOS;

namespace Rocks.Wasabee.Mobile.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : MvxFormsApplicationDelegate<Setup, CoreApp, App>
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
            
            var platformConfig = new PlatformConfig() { ImageFactory = new WasabeeImageFactory() };
            Xamarin.FormsGoogleMaps.Init(MapsKey.Value, platformConfig);
            
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageSourceHandler();

            // Use Firebase library to configure APIs
            Firebase.Core.App.Configure();
 
            return base.FinishedLaunching(app, options);
        }

        public override void DidEnterBackground (UIApplication application)
        {
            Console.WriteLine ("App entering background state.");
        }

        public override void WillEnterForeground (UIApplication application)
        {
            Console.WriteLine ("App will enter foreground");
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                if (Mvx.IoCProvider.CanResolve<ILoggingService>())
                {
                    Mvx.IoCProvider.Resolve<ILoggingService>().Fatal(exception, "[CurrentDomain] Fatal error occured");
                    throw exception;
                }
            }
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (Mvx.IoCProvider.CanResolve<ILoggingService>())
            {
                Mvx.IoCProvider.Resolve<ILoggingService>().Fatal(e.Exception, "[TaskScheduler] Fatal error occured");
                throw e.Exception;
            }
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (Xamarin.Essentials.Platform.OpenUrl(app, url, options))
                return true;
    
            return base.OpenUrl(app, url, options);
        }
    }
}
