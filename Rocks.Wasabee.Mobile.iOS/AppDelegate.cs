﻿using Foundation;
using Rocks.Wasabee.Mobile.Core.Ui;
using UIKit;
using Xamarin.Forms.GoogleMaps.iOS;

namespace Rocks.Wasabee.Mobile.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
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
            Xamarin.Forms.Forms.Init();
            Xamarin.FormsGoogleMaps.Init("", new PlatformConfig() { });

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
