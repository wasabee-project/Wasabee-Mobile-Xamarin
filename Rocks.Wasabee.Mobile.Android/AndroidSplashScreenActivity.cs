using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MvvmCross.Forms.Platforms.Android.Views;
using Rocks.Wasabee.Mobile.Core;

namespace Rocks.Wasabee.Mobile.Droid
{
    [Activity(
        Theme = "@style/Theme.Splash",
        ResizeableActivity = true,
        MainLauncher = true,
        WindowSoftInputMode = SoftInput.AdjustPan,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AndroidSplashScreenActivity : MvxFormsSplashScreenAppCompatActivity<Setup, CoreApp, App>
    {
        protected override Task RunAppStartAsync(Bundle bundle)
        {
            var intent = new Intent(this, typeof(AndroidMainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
            StartActivity(intent);

            Finish();

            return Task.CompletedTask;
        }

        public override void OnBackPressed()
        {
            // Do nothing
        }
    }
}