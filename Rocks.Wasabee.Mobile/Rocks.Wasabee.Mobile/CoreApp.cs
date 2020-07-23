using MvvmCross.IoC;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.ViewModels;

namespace Rocks.Wasabee.Mobile.Core
{
    public class CoreApp : MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

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

            RegisterAppStart<SplashScreenViewModel>();
        }
    }
}