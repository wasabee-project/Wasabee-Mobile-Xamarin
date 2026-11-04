using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Refit;
using Rocks.Wasabee.Core;
using Rocks.Wasabee.Core.Adapters;
using Rocks.Wasabee.Core.Infra.Security;
using Rocks.Wasabee.Core.Services;
using Rocks.Wasabee.Core.Settings.Application;
using Rocks.Wasabee.Core.Settings.User;
using Rocks.Wasabee.Core.ViewModels;
using Rocks.Wasabee.Maui.Core.Adapters;
using Rocks.Wasabee.Maui.Core.Services;
using Rocsk.Wasabee.Maui.Core.Services;

namespace Rocks.Wasabee.Maui
{
    public static class MauiProgram
    {
        public static CoreApp CoreApp;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiCommunityToolkit()
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .SetupAppSettings()
                .ConfigureWasabeeRefitHttpClient()
                .RegisterAdapters()
                .RegisterProviders()
                .RegisterServices()
                .RegisterViewModels();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var mauiApp = builder.Build();
            CoreApp = new CoreApp(mauiApp.Services);

            return mauiApp;
        }

        private static MauiAppBuilder SetupAppSettings(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<IAppSettings, DevAppSettings>();

            return builder;
        }

        private static MauiAppBuilder ConfigureWasabeeRefitHttpClient(this MauiAppBuilder builder)
        {
            builder.Services.AddTransient<WasabeeTokenHandler>();
            builder.Services.AddRefitClient<IWasabeeApiV1>()
                .ConfigureHttpClient((_, client) =>
                {
                    // Required by Refit to start, the actual request URI will be set by the WasabeeTokenHandler based on the selected server.
                    client.BaseAddress = new Uri("https://wasabee.local");
                })
                .AddHttpMessageHandler<WasabeeTokenHandler>();

            return builder;
        }

        private static MauiAppBuilder RegisterAdapters(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<ISecureStorageAdapter, SecureStorageAdapter>();
            builder.Services.AddSingleton<IPreferencesAdapter, PreferencesAdapter>();

            return builder;
        }

        private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<WasabeeApiV1Service>();

            builder.Services.AddSingleton<IAuthentificationService, AuthentificationService>();
            builder.Services.AddSingleton<IUserSettingsService, UserSettingsService>();

            return builder;
        }

        private static MauiAppBuilder RegisterProviders(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<ILoginProvider, LoginProvider>();

            return builder;
        }

        private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<LoginPageViewModel>();

            return builder;
        }
    }
}
