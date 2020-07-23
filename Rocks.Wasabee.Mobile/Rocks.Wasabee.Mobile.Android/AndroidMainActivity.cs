using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.OS;
using MvvmCross.Forms.Platforms.Android.Views;
using Rocks.Wasabee.Mobile.Core;
namespace Rocks.Wasabee.Mobile.Droid
{
    [Activity(
        Theme = "@style/MainTheme",
        ResizeableActivity = true,
        WindowSoftInputMode = SoftInput.AdjustPan,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AndroidMainActivity : MvxFormsAppCompatActivity<Setup, CoreApp, App>
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.Essentials.Platform.Init(this, bundle);

            base.OnCreate(bundle);

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("[DEBUG] Activated WindowManagerFlags.KeepScreenOn while Debugger is connected");
                Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            }
            else
            {
                Console.WriteLine("[DEBUG] Removed WindowManagerFlags.KeepScreenOn");
                Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
            }
#endif
        }

        protected override void OnResume()
        {
            base.OnResume();

            Xamarin.Essentials.Platform.OnResume();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            // Do nothing
        }
    }
}