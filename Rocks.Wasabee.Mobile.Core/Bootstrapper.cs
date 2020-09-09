using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core
{
    public static class Bootstrapper
    {
        public static void SetupCrossPlugins()
        {
            Mvx.IoCProvider.RegisterSingleton<IPreferences>(() => new PreferencesImplementation());
            Mvx.IoCProvider.RegisterSingleton<IConnectivity>(() => new ConnectivityImplementation());
            Mvx.IoCProvider.RegisterSingleton<IPermissions>(() => new PermissionsImplementation());
            Mvx.IoCProvider.RegisterSingleton<IVersionTracking>(() => new VersionTrackingImplementation());
            Mvx.IoCProvider.RegisterSingleton<ISecureStorage>(() => new SecureStorageImplementation());
            Mvx.IoCProvider.RegisterSingleton<IFileSystem>(() => new FileSystemImplementation());
            Mvx.IoCProvider.RegisterSingleton<IGeolocation>(() => new GeolocationImplementation());
        }

        public static void SetupCrossConcerns()
        {
            Mvx.IoCProvider.RegisterSingleton<IMvxMessenger>(new MvxMessengerHub());
        }

        public static void SetupEnvironment()
        {
            var environnement = string.Empty;
#if DEBUG
            environnement = "debug";
#else
            environnement = "release";
#endif

            Mvx.IoCProvider.Resolve<IPreferences>().Set("appEnvironnement", environnement);
        }

        public static void SetupAppSettings()
        {
#if DEBUG
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IAppSettings, DevAppSettings>();
#else
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IAppSettings, ProdAppSettings>();
#endif
        }

        public static void SetupServices()
        {
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ILoggingService, LoggingService>();

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton(() => new WasabeeApiV1Service(Mvx.IoCProvider.Resolve<IAppSettings>()));
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IUserSettingsService, UserSettingsService>();

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ILoginProvider, LoginProvider>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IAuthentificationService, AuthentificationService>();
        }

        public static void SetupDatabases()
        {
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton(() => new UsersDatabase(Mvx.IoCProvider.Resolve<IFileSystem>(), Mvx.IoCProvider.Resolve<ILoggingService>()));
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton(() => new OperationsDatabase(Mvx.IoCProvider.Resolve<IFileSystem>(), Mvx.IoCProvider.Resolve<ILoggingService>()));
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton(() => new TeamsDatabase(Mvx.IoCProvider.Resolve<IFileSystem>(), Mvx.IoCProvider.Resolve<ILoggingService>()));
        }
    }
}