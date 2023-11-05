using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Common;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Play.Core.Appupdate;
using Com.Google.Android.Play.Core.Install.Model;
using Com.Google.Android.Play.Core.Tasks;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Views;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Ui;
using Rocks.Wasabee.Mobile.Core.Ui.Themes;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Droid
{
    [Activity(
        Theme = "@style/Theme.Splash",
        Icon = "@mipmap/ic_launcher",
        RoundIcon = "@mipmap/ic_launcher_round",
        ResizeableActivity = true,
        MainLauncher = true,
        Exported = true,
        WindowSoftInputMode = SoftInput.AdjustPan,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
    public class AndroidSplashScreenActivity : MvxFormsSplashScreenActivity<Setup, CoreApp, App>
    {
        private const int RequestUpdate = 1337;
        
        protected override async System.Threading.Tasks.Task RunAppStartAsync(Bundle bundle)
        {
            if (!await IsPlayServicesAvailable()) return;
            
            SetAppTheme();

            var intent = new Intent(this, typeof(AndroidMainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);

#if DEBUG
            StartActivity(intent);
            Finish();
#else
            IAppUpdateManager appUpdateManager = AppUpdateManagerFactory.Create(this);
            var appUpdateInfoTask = appUpdateManager.AppUpdateInfo;
            appUpdateInfoTask.AddOnSuccessListener(new AppUpdateSuccessListener(appUpdateManager, this, RequestUpdate, intent));
#endif
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            
            if (RequestUpdate.Equals(requestCode))
            {
                switch (resultCode) // The switch block will be triggered only with flexible update since it returns the install result codes
                {
                    case Result.Ok:
                        // In app update success
                        Mvx.IoCProvider.Resolve<ILoggingService>().Info("[In-App Update] Application updated !");
                        Toast.MakeText(this, "Application updated", ToastLength.Short)?.Show();
                        break;
                    case Result.Canceled:
                        Mvx.IoCProvider.Resolve<ILoggingService>().Warn("[In-App Update] Update cancelled");
                        Toast.MakeText(this, "Application update cancelled", ToastLength.Short)?.Show();
                        break;
                    default:
                        Mvx.IoCProvider.Resolve<ILoggingService>().Warn("[In-App Update] Update failed");
                        Toast.MakeText(this, "Application update failed", ToastLength.Short)?.Show();
                        break;
                }
            }
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

    public class AppUpdateSuccessListener : Java.Lang.Object, IOnSuccessListener
    {
        private readonly IAppUpdateManager _appUpdateManager;
        private readonly Activity _splashActivity;
        private readonly int _updateRequest;
        private readonly Intent _intent;

        public AppUpdateSuccessListener(IAppUpdateManager appUpdateManager, Activity splashActivity, int updateRequest, Intent intent)
        {
            _appUpdateManager = appUpdateManager;
            _splashActivity = splashActivity;
            _updateRequest = updateRequest;
            _intent = intent;
        }

        public void OnSuccess(Java.Lang.Object p0)
        {
            if (!(p0 is AppUpdateInfo info))
                return;

            var availability = info.UpdateAvailability();
            if ((availability.Equals(UpdateAvailability.UpdateAvailable) || availability.Equals(UpdateAvailability.DeveloperTriggeredUpdateInProgress)) && info.IsUpdateTypeAllowed(AppUpdateType.Immediate))
            {
                // Start an update
                Mvx.IoCProvider.Resolve<ILoggingService>().Info("[In-App Update] STARTING UPDATE");
                Toast.MakeText(_splashActivity, "Update available, started !", ToastLength.Short)?.Show();
                _appUpdateManager.StartUpdateFlowForResult(info, AppUpdateType.Immediate, _splashActivity, _updateRequest);
            }

            if (availability.Equals(UpdateAvailability.UpdateNotAvailable) || availability.Equals(UpdateAvailability.Unknown))
            {
                // No update available, continue app start
                Mvx.IoCProvider.Resolve<ILoggingService>().Info("[In-App Update] No update available, continue app start");
                Toast.MakeText(_splashActivity, "No update available", ToastLength.Short)?.Show();
                _splashActivity.StartActivity(_intent);
                _splashActivity.Finish();
            }
        }
    }
}