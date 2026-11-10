using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Refit;
using Rocks.Wasabee.Core.Adapters;
using Rocks.Wasabee.Core.Infra.Databases;
using Rocks.Wasabee.Core.Infra.Security;
using Rocks.Wasabee.Core.Services;
using Rocks.Wasabee.Core.Settings.Application;
using Rocks.Wasabee.Core.Settings.User;
using Rocks.Wasabee.Core.ViewModels;
using Rocks.Wasabee.Maui.Core.Adapters;
using Rocks.Wasabee.Maui.Core.Services;
using Rocsk.Wasabee.Maui.Core.Services;
using Rocks.Wasabee.Maui.Views;
using Rocks.Wasabee.Core.ViewModels.Operation;


#if ANDROID
using Maui.GoogleMaps.Android;
using Maui.GoogleMaps.Hosting;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using Android.Content.Res;
using Android.Gms.Maps.Model;
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
#if ANDROID
                .UseGoogleMaps(new PlatformConfig() { BitmapDescriptorFactory = new WasabeeBitmapConfig() })
#endif
                .SetupAppSettings()
                .ConfigureWasabeeRefitHttpClient()
                .RegisterAdapters()
                .RegisterProviders()
                .RegisterServices()
                .RegisterDatabases()
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

        extension(MauiAppBuilder builder)
        {
            private MauiAppBuilder SetupAppSettings()
            {
                builder.Services.AddSingleton<IAppSettings, DevAppSettings>();

                return builder;
            }

            private MauiAppBuilder ConfigureWasabeeRefitHttpClient()
            {
                builder.Services.AddSingleton<WasabeeTokenHandler>();
                builder.Services.AddRefitClient<IWasabeeApiV1>(new RefitSettings { ContentSerializer = new SystemTextJsonContentSerializer() })
                    .ConfigureHttpClient((_, client) =>
                    {
                        // Required by Refit to start, the actual request URI will be set by the WasabeeTokenHandler based on the selected server.
                        client.BaseAddress = new Uri("https://wasabee.local");
                    })
                    .AddHttpMessageHandler<WasabeeTokenHandler>();

                return builder;
            }

            private MauiAppBuilder RegisterAdapters()
            {
                builder.Services.AddSingleton<ISecureStorageAdapter, SecureStorageAdapter>();
                builder.Services.AddSingleton<IPreferencesAdapter, PreferencesAdapter>();
                builder.Services.AddSingleton<IFileSystemAdapter, FileSystemAdapter>();
                builder.Services.AddSingleton<IToastAdapter, ToastAdapter>();
                builder.Services.AddSingleton<IConnectivityAdapter, ConnectivityAdapter>();
                builder.Services.AddSingleton<IMessengerAdapter, MessengerAdapter>();

                builder.Services.AddSingleton<INavigationService, NavigationAdapter>();

                return builder;
            }

            private MauiAppBuilder RegisterServices()
            {
                builder.Services.AddSingleton<WasabeeApiV1Service>();

                builder.Services.AddSingleton<IAuthentificationService, AuthentificationService>();
                builder.Services.AddSingleton<IUserSettingsService, UserSettingsService>();

                return builder;
            }

            private MauiAppBuilder RegisterProviders()
            {
                builder.Services.AddSingleton<ILoginProvider, LoginProvider>();

                return builder;
            }

            private MauiAppBuilder RegisterDatabases()
            {
                builder.Services.AddSingleton<UsersDatabase>();

                builder.Services.AddSingleton<MarkersDatabase>();
                builder.Services.AddSingleton<LinksDatabase>();
                builder.Services.AddSingleton<OperationsDatabase>();

                builder.Services.AddSingleton<AgentsDatabase>();
                builder.Services.AddSingleton<TeamsDatabase>();

                return builder;
            }

            private MauiAppBuilder RegisterViewModels()
            {
                builder.Services.AddTransient<LoginPage, LoginPageViewModel>();
                builder.Services.AddTransient<OperationMapPage, OperationMapViewModel>();

                return builder;
            }
        }
    }
}
