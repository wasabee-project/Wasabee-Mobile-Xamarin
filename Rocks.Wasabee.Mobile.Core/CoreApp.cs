using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core
{
    public class CoreApp : MvxApplication
    {
        public override async void Initialize()
        {
            CreatableTypes()
                .EndingWith("ViewModel")
                .Except(typeof(BaseViewModel))
                .AsTypes()
                .RegisterAsLazySingleton();

            Bootstrapper.SetupCrossPlugins();
            Bootstrapper.SetupCrossConcerns();
            Bootstrapper.SetupEnvironment();
            Bootstrapper.SetupAppSettings();
            Bootstrapper.SetupDatabases();
            Bootstrapper.SetupServices();

#if DEBUG
            Mvx.IoCProvider.Resolve<IPreferences>().Set(UserSettingsKeys.DevModeActivated, true);
#endif

            AppCenter.Start(
                $"android={Mvx.IoCProvider.Resolve<IAppSettings>().AndroidAppCenterKey};"
                // TODO + "ios={Your iOS App secret here}"
                , typeof(Crashes), typeof(Analytics));

            var analyticsEnabled = Mvx.IoCProvider.Resolve<IPreferences>().Get(UserSettingsKeys.AnalyticsEnabled, false);
            if (!analyticsEnabled)
            {
                await Crashes.SetEnabledAsync(false);
                await Analytics.SetEnabledAsync(false);
            }

            Analytics.TrackEvent(AnalyticsConstants.AppStart);
            NLog.LogManager.EnableLogging();

            RegisterAppStart<SplashScreenViewModel>();
        }
    }
}