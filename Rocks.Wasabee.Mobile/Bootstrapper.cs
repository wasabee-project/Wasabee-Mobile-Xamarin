using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core
{
    public static class Bootstrapper
    {
        public static void SetupCrossConcerns()
        {
            Mvx.IoCProvider.RegisterSingleton<IMvxMessenger>(new MvxMessengerHub());
            
            Mvx.IoCProvider.RegisterType<ILoginProvider, LoginProvider>();
        }

        public static void SetupAppSettings()
        {
#if !PROD
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IAppSettings, DevAppSettings>();
#else
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IAppSettings, ProdAppSettings>();
#endif
        }

        public static void SetupCrossPlugins()
        {
            Mvx.IoCProvider.RegisterSingleton<IPreferences>(() => new PreferencesImplementation());
            Mvx.IoCProvider.RegisterSingleton<IConnectivity>(() => new ConnectivityImplementation());
            Mvx.IoCProvider.RegisterSingleton<IPermissions>(() => new PermissionsImplementation());
            Mvx.IoCProvider.RegisterSingleton<IVersionTracking>(() => new VersionTrackingImplementation());
        }

        public static void SetupServices()
        {
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IUserSettingsService, UserSettingsService>();
        }

        public static void SetupEnvironment()
        {
            var environnement = string.Empty;
#if DEBUG
            environnement = "debug";
#elif DEV
            environnement = "dev";
#elif PROD
            environnement = "prod";
#endif

            Mvx.IoCProvider.Resolve<IPreferences>().Set("appEnvironnement", environnement);
        }
    }
}