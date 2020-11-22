using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Common;
using Android.OS;
using Android.Util;
using Android.Views;
using MvvmCross.Forms.Platforms.Android.Views;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Ui;
using Rocks.Wasabee.Mobile.Core.Ui.Themes;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Droid
{
    [Activity(
        Theme = "@style/Theme.Splash",
#if DEBUG
        Icon = "@drawable/wasabeedev",
#else
        Icon = "@drawable/wasabee",
#endif
        ResizeableActivity = true,
        MainLauncher = true,
        WindowSoftInputMode = SoftInput.AdjustPan,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
    public class AndroidSplashScreenActivity : MvxFormsSplashScreenActivity<Setup, CoreApp, App>
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

            SetAppTheme();

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

        private void SetAppTheme()
        {
            if (Resources?.Configuration != null && Resources.Configuration.UiMode.HasFlag(UiMode.NightYes))
                SetTheme(Core.Messages.Theme.Dark);
            else
                SetTheme(Core.Messages.Theme.Light);
        }

        private void SetTheme(Core.Messages.Theme mode)
        {
            if (mode == Core.Messages.Theme.Dark)
            {
                if (CoreApp.AppTheme == Core.Messages.Theme.Dark)
                    return;

                App.Current.Resources = new DarkTheme();
            }
            else
            {
                if (CoreApp.AppTheme != Core.Messages.Theme.Dark)
                    return;

                App.Current.Resources = new LightTheme();
            }

            CoreApp.AppTheme = mode;
        }
    }
}