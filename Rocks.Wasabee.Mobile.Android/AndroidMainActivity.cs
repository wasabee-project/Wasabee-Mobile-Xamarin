using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using FFImageLoading.Forms.Platform;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Views;
using MvvmCross.Plugin.Messenger;
using Rg.Plugins.Popup.Contracts;
using Rg.Plugins.Popup.Services;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Ui;
using Rocks.Wasabee.Mobile.Core.Ui.Services;
using Rocks.Wasabee.Mobile.Droid.Services.Geolocation;
using System;
using System.Collections.Generic;
using Xamarin.Forms.GoogleMaps.Android;
using Action = Rocks.Wasabee.Mobile.Core.Messages.Action;

namespace Rocks.Wasabee.Mobile.Droid
{
    [Activity(
        Theme = "@style/MainTheme",
        ResizeableActivity = true,
        WindowSoftInputMode = SoftInput.AdjustPan,
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AndroidMainActivity : MvxFormsAppCompatActivity<Setup, CoreApp, App>
    {
        public const string CHANNEL_ID = "WASABEE_FCM_CHANNEL";

        private MvxSubscriptionToken _token;

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
                Rg.Plugins.Popup.Popup.Init(this, bundle);

                Xamarin.Forms.Forms.Init(this, bundle);
                Xamarin.Essentials.Platform.Init(this, bundle);

                var platformConfig = new PlatformConfig() { BitmapDescriptorFactory = new WasabeeBitmapConfig() };
                Xamarin.FormsGoogleMaps.Init(this, bundle, platformConfig);

                Plugin.Iconize.Iconize.Init(Resource.Id.toolbar, Resource.Id.sliding_tabs);

                CachedImageRenderer.Init(true);
                CachedImageRenderer.InitImageViewHandler();

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

            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            Xamarin.Essentials.Platform.OnResume();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override async void OnBackPressed()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                await PopupNavigation.Instance.PopAsync();
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
    }
}