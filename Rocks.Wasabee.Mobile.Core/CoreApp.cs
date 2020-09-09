using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.ViewModels;

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
            Bootstrapper.SetupServices();
            Bootstrapper.SetupDatabases();

            AppCenter.Start(
                $"android={Mvx.IoCProvider.Resolve<IAppSettings>().AndroidAppCenterKey};"
                // TODO + "ios={Your iOS App secret here}"
                , typeof(Crashes), typeof(Analytics));

            Analytics.TrackEvent(AnalyticsConstants.AppStart);

            RegisterAppStart<SplashScreenViewModel>();
        }
    }
}