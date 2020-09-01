using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using MvvmCross;
using MvvmCross.Forms.Platforms.Android.Views;
using MvvmCross.Plugin.Messenger;
using Rocks.Wasabee.Mobile.Core;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Ui;
using Rocks.Wasabee.Mobile.Droid.Services.Geolocation;
using System;
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

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            if (Intent.Extras != null && Intent.Extras.ContainsKey("FCMMessage"))
            {
                foreach (var key in Intent.Extras.KeySet())
                {
                    var value = Intent.Extras.GetString(key);
                    Log.Debug("AndroidMainActivity", "Key: {0} Value: {1}", key, value);
                }

                base.OnCreate(bundle);
            }
            else
            {
                Xamarin.Forms.Forms.Init(this, bundle);
                Xamarin.Essentials.Platform.Init(this, bundle);

                var platformConfig = new PlatformConfig
                {
                    BitmapDescriptorFactory = new WasabeeBitmapConfig()
                };
                Xamarin.FormsGoogleMaps.Init(this, bundle, platformConfig);

                Plugin.Iconize.Iconize.Init(Resource.Id.toolbar, Resource.Id.sliding_tabs);

                CreateNotificationChannels();

                base.OnCreate(bundle);

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

                Mvx.IoCProvider.Resolve<IMvxMessenger>().Subscribe<LiveGeolocationTrackingMessage>(async msg =>
                {
                    if (msg.Action == Action.Start)
                        await GeolocationHelper.StartLocationService();
                    else
                        GeolocationHelper.StopLocationService();
                });
            }
        }

        const int RequestLocationId = 0;

        readonly string[] _locationPermissions =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        protected override void OnStart()
        {
            base.OnStart();

            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    RequestPermissions(_locationPermissions, RequestLocationId);
                }
                else
                {
                    // Permissions already granted - display a message.
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
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            // Do nothing
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

                notificationManager.CreateNotificationChannel(channel);
            }
        }
    }
}