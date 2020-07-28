using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.OS;
using Android.Util;
using Android.Views;
using MvvmCross.Forms.Platforms.Android.Views;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Ui;
using System.Threading.Tasks;

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
        protected override async Task RunAppStartAsync(Bundle bundle)
        {
            if (!await IsPlayServicesAvailable()) return;

            var intent = new Intent(this, typeof(AndroidMainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);

            if (Intent.Extras != null && Intent.Extras.ContainsKey("FCMMessage"))
            {
                foreach (var key in Intent.Extras.KeySet())
                {
                    var value = Intent.Extras.GetString(key);
                    intent.PutExtra(key, value);

                    Log.Debug("AndroidSplashScreenActivity", "Key: {0} Value: {1}", key, value);
                }
            }

            StartActivity(intent);

            Finish();
        }

        public override void OnBackPressed()
        {
            // Do nothing
        }

        private Task<bool> IsPlayServicesAvailable()
        {
            TaskCompletionSource<bool> taskCompletitionSource = new TaskCompletionSource<bool>();
            var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode == ConnectionResult.Success)
            {
                taskCompletitionSource.TrySetResult(true);
                return taskCompletitionSource.Task;
            }

            var alertDialog = new AlertDialog.Builder(this).Create();
            alertDialog.SetTitle("Google Play Services");
            alertDialog.SetButton("Ok", (e, o) =>
            {
                Finish();
                taskCompletitionSource.TrySetResult(false);
            });

            alertDialog.SetMessage(GoogleApiAvailability.Instance.IsUserResolvableError(resultCode)
                ? GoogleApiAvailability.Instance.GetErrorString(resultCode)
                : "This device is not supported");

            alertDialog.Show();
            return taskCompletitionSource.Task;
        }
    }
}