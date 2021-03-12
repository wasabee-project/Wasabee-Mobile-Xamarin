using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.App;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Views;
using MvvmCross.Forms.Views;
using MvvmCross.Plugin.Messenger;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Services;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Ui;
using Rocks.Wasabee.Mobile.Core.Ui.Services;
using Rocks.Wasabee.Mobile.Core.Ui.Themes;
using Rocks.Wasabee.Mobile.Droid.Services.Geolocation;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps.Android;
using Action = Rocks.Wasabee.Mobile.Core.Messages.Action;
using Orientation = Rocks.Wasabee.Mobile.Core.Messages.Orientation;

namespace Rocks.Wasabee.Mobile.Droid
{
    [Activity(
        Theme = "@style/MainTheme",
        ResizeableActivity = true,
        WindowSoftInputMode = SoftInput.AdjustPan,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
    public class AndroidMainActivity : MvxFormsAppCompatActivity<Setup, CoreApp, App>
    {
        public const string CHANNEL_ID = "WASABEE_FCM_CHANNEL";

        private MvxSubscriptionToken _token;
        private MvxSubscriptionToken _tokenTheme;
        private MvxSubscriptionToken _tokenOrientation;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            if (Intent?.Extras != null && Intent.Extras.ContainsKey("FCMMessage"))
            {
                var keySet = Intent.Extras.KeySet() ?? new List<string>();
                foreach (var key in keySet)
                {
                    var value = Intent.Extras.GetString(key) ?? "empty 'value'";
                    Log.Debug("AndroidMainActivity", "Key: {0} Value: {1}", key, value);
                }

                base.OnCreate(bundle);
            }
            else if (Intent?.Extras != null && Intent.Extras.ContainsKey("LiveGeolocationTrackingExtra"))
            {
                base.OnCreate(bundle);
            }
            else
            {
                SetAppTheme();

                Xamarin.Forms.Forms.Init(this, bundle);
                Xamarin.Essentials.Platform.Init(this, bundle);

                Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);

                Rg.Plugins.Popup.Popup.Init(this);
                ZXing.Net.Mobile.Forms.Android.Platform.Init();
                ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);

                var platformConfig = new PlatformConfig() { BitmapDescriptorFactory = new WasabeeBitmapConfig() };
                Xamarin.FormsGoogleMaps.Init(this, bundle, platformConfig);

                Plugin.Iconize.Iconize.Init(Resource.Id.toolbar, Resource.Id.sliding_tabs);

                FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
                FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageViewHandler();

                CreateNotificationChannels();

                Mvx.IoCProvider.RegisterSingleton<IPopupNavigation>(PopupNavigation.Instance);
                Mvx.IoCProvider.RegisterType<IDialogNavigationService, DialogNavigationService>();

                base.OnCreate(bundle);

                _token = Mvx.IoCProvider.Resolve<IMvxMessenger>().Subscribe<LiveGeolocationTrackingMessage>(async msg =>
                {
                    if (msg.Action == Action.Start)
                        await GeolocationHelper.StartLocationService();
                    else
                        GeolocationHelper.StopLocationService();
                });

                _tokenTheme = Mvx.IoCProvider.Resolve<IMvxMessenger>().Subscribe<ChangeThemeMessage>(msg => OnThemeChanged(msg.Theme));
                _tokenOrientation = Mvx.IoCProvider.Resolve<IMvxMessenger>().Subscribe<ChangeOrientationMessage>(msg =>
                {
                    if (msg.Orientation == Orientation.Portait)
                        RequestedOrientation = ScreenOrientation.Portrait;
                    else if (msg.Orientation == Orientation.Landscape)
                        RequestedOrientation = ScreenOrientation.Landscape;
                    else if (msg.Orientation == Orientation.Any)
                        RequestedOrientation = ScreenOrientation.Unspecified;
                });

#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("[DEBUG] Activated WindowManagerFlags.KeepScreenOn while Debugger is connected");
                    Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                else
                {
                    Console.WriteLine("[DEBUG] Removed WindowManagerFlags.KeepScreenOn");
                    Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
                }
#endif

                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    try
                    {
                        var dozeIntent = new Intent();
                        dozeIntent.SetAction(Settings.ActionRequestIgnoreBatteryOptimizations);
                        dozeIntent.SetData(Android.Net.Uri.Parse("package:" + Plugin.CurrentActivity.CrossCurrentActivity.Current.AppContext.PackageName));
                        StartActivity(dozeIntent);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            Xamarin.Essentials.Platform.OnResume();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override async void OnBackPressed()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                await PopupNavigation.Instance.PopAsync();
            }
            else if (App.Current.MainPage.Navigation.ModalStack.Count > 0)
            {
                await App.Current.MainPage.Navigation.PopModalAsync(true);
            }
            else if (App.Current.MainPage is NavigationPage {CurrentPage: MvxMasterDetailPage {Detail: NavigationPage detailNavigationPage}} && detailNavigationPage.Pages.Count() > 1)
            {
                await detailNavigationPage.Navigation.PopAsync(true);
            }
            else
            {
                // Do nothing
            }
        }

        private void CreateNotificationChannels()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);

            var channels = new[]
            {
                "Quit", "Generic Message", "Agent Location Change", "Map Change", "Marker Status Change",
                "Marker Assignment Change", "Link Status Change", "Link Assignment Change", "Subscribe", "Login", "Undefined"
            };

            foreach (var name in channels)
            {
                var channel = new NotificationChannel(name, name, NotificationImportance.Default)
                {
                    Description = $"Firebase Cloud Messages '{name}' in this channel"
                };

                notificationManager?.CreateNotificationChannel(channel);
            }
        }

        private void OnThemeChanged(Theme theme)
        {
            if (theme == Core.Messages.Theme.Light)
            {
                Delegate.SetLocalNightMode(AppCompatDelegate.ModeNightNo);
            }
            else
            {
                Delegate.SetLocalNightMode(AppCompatDelegate.ModeNightYes);
            }
            SetTheme(theme);
        }

        private void SetAppTheme()
        {
            if (Resources?.Configuration != null && Resources.Configuration.UiMode.HasFlag(UiMode.NightYes))
                SetTheme(Core.Messages.Theme.Dark);
            else
                SetTheme(Core.Messages.Theme.Light);
        }

        private void SetTheme(Theme mode)
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