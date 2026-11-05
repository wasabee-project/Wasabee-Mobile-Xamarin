using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Refit;
using Rocks.Wasabee.Core.Adapters;
using Rocks.Wasabee.Core.Infra.Security;
using Rocks.Wasabee.Core.Services;
using Rocks.Wasabee.Core.Settings.Application;
using Rocks.Wasabee.Core.Settings.User;
using Rocks.Wasabee.Core.ViewModels;
using Rocks.Wasabee.Maui.Core.Adapters;
using Rocks.Wasabee.Maui.Core.Services;
using Rocsk.Wasabee.Maui.Core.Services;


#if ANDROID
using Microsoft.Maui.Platform;
using Android.Content.Res;
#endif

#if IOS
using UIKit;
#endif

namespace Rocks.Wasabee.Maui
{
    public static class MauiProgram
    {
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
                    fonts.AddFont("SpaceMono-Regular.ttf", "SpaceMonoRegular");
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
            EntryHandler.Mapper.AppendToMapping("EntryCustomizations", (handler, _) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Android.Graphics.Color.Transparent);

                if (Application.Current?.Resources.TryGetValue("PrimaryGreenDark", out var colorObj) is true && colorObj is Color xamlColor)
                {
                    handler.PlatformView.TextCursorDrawable.SetTint(xamlColor.ToPlatform());
                    handler.PlatformView.SetHighlightColor(xamlColor.WithAlpha(0.3f).ToPlatform());

                    handler.PlatformView.TextSelectHandle?.SetTint(xamlColor.ToPlatform());
                    handler.PlatformView.TextSelectHandleLeft?.SetTint(xamlColor.ToPlatform());
                    handler.PlatformView.TextSelectHandleRight?.SetTint(xamlColor.ToPlatform());
                }
#elif IOS
                handler.PlatformView.BorderStyle = UITextBorderStyle.None;
#endif
            });

            return builder.Build();
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
