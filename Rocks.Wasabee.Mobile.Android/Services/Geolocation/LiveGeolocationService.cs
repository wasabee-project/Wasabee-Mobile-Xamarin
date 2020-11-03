#nullable enable
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using MvvmCross;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Droid.Services.Geolocation
{
    public class LiveGeolocationServiceBinder : Binder
    {
        public LiveGeolocationServiceBinder(LiveGeolocationService service)
        {
            Service = service;
        }

        public LiveGeolocationService Service { get; }

        public bool IsBound { get; set; }
    }

    [Service(DirectBootAware = false, Enabled = true, Exported = false, ForegroundServiceType = ForegroundService.TypeLocation)]
    public class LiveGeolocationService : Service
    {
        private static CultureInfo Culture => CultureInfo.GetCultureInfo("en-US");
        private const string ChannelId = "Wasabee Live Location Sharing";
        private const int NotificationId = 1337;

        private IBinder? _binder;
        private WasabeeApiV1Service? _wasabeeApiV1Service;
        private ILoggingService? _loggingService;
        private IPreferences? _preferences;

        private bool _isRunning;

        private static IGeolocator Geolocator => CrossGeolocator.Current;

        public override IBinder OnBind(Intent? intent)
        {
            _binder = new LiveGeolocationServiceBinder(this);
            return _binder;
        }

        public override bool OnUnbind(Intent? intent)
        {
            StopLocationUpdates().ConfigureAwait(false);
            return base.OnUnbind(intent);
        }

        public override bool StopService(Intent? name)
        {
            StopLocationUpdates().ConfigureAwait(false);
            return base.StopService(name);
        }

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            CreateNotificationChannel();

            StartForeground(NotificationId, CreateNotification());

            return StartCommandResult.Sticky;
        }

        public async Task StartLocationUpdates()
        {
            if (_isRunning)
                return;

            try
            {
                _isRunning = true;

                _wasabeeApiV1Service ??= Mvx.IoCProvider.Resolve<WasabeeApiV1Service>();
                _preferences ??= Mvx.IoCProvider.Resolve<IPreferences>();

                if (Geolocator.IsListening)
                {
                    await Geolocator.StopListeningAsync();
                }

                if (Geolocator.IsGeolocationAvailable && Geolocator.IsGeolocationEnabled)
                {
                    Geolocator.DesiredAccuracy = 5;
                    Geolocator.PositionChanged += Geolocator_PositionChanged;

                    //every 3 second, 5 meters
                    await Geolocator.StartListeningAsync(TimeSpan.FromSeconds(3), 5);
                }
                else
                {
                    Mvx.IoCProvider.Resolve<IUserDialogs>().Alert(
                        "Please ensure that geolocation is enabled and permissions are allowed for Wasabee to start sharing your location.",
                        "Geolocation Disabled", "OK");
                }
            }
            catch (Exception e)
            {
                Mvx.IoCProvider.Resolve<ILoggingService>().Error(e, e.Message);
                _isRunning = false;
            }
        }

        private async void Geolocator_PositionChanged(object sender, PositionEventArgs e)
        {
            if (!_isRunning)
                return;
            
            _preferences ??= Mvx.IoCProvider.Resolve<IPreferences>();
            if (_preferences.Get(UserSettingsKeys.LiveLocationSharingEnabled, false) == false)
            {
                GeolocationHelper.StopLocationService();
                return;
            }

            try
            {
                _preferences.Set(UserSettingsKeys.LiveLocationSharingEnabled, true);

                var result = await _wasabeeApiV1Service!.User_UpdateLocation(e.Position.Latitude.ToString(Culture), e.Position.Longitude.ToString(Culture));
                if (result)
                {
                    // Updates notification with latest update time
                    var notification = CreateNotification();
                    var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
                    notificationManager.Notify(NotificationId, notification);
                }
            }
            catch (Exception ex)
            {
                _loggingService ??= Mvx.IoCProvider.Resolve<ILoggingService>();
                _loggingService.Error(ex, "Error Executing LiveGeolocationService.Geolocator_PositionChanged");
            }
        }

        public async Task StopLocationUpdates()
        {
            if (_isRunning)
            {
                if (await Geolocator.StopListeningAsync())
                {
                    Geolocator.PositionChanged -= Geolocator_PositionChanged;
                    _isRunning = false;
                    
                    _preferences ??= Mvx.IoCProvider.Resolve<IPreferences>();
                    _preferences.Set(UserSettingsKeys.LiveLocationSharingEnabled, false);
                }
            }
        }

        private Notification CreateNotification()
        {
            var builder = new NotificationCompat.Builder(this, ChannelId);

            var newIntent = new Intent(this, typeof(AndroidMainActivity));
            newIntent.PutExtra("LiveGeolocationTrackingExtra", true);
            newIntent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

            var pendingIntent = PendingIntent.GetActivity(this, 0, newIntent, PendingIntentFlags.UpdateCurrent);
            var notification = builder.SetContentIntent(pendingIntent)
                .SetSmallIcon(Resource.Drawable.wasabee)
                .SetAutoCancel(false)
                .SetTicker("Wasabee location sharing")
                .SetContentTitle("Wasabee")
                .SetContentText($"Wasabee is sharing your location.\r\nLast update at {DateTime.Now:T}")
                .SetChannelId(ChannelId)
                .SetSound(null)
                .Build();

            return notification;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;

            var channel = new NotificationChannel(ChannelId, ChannelId, NotificationImportance.Default)
            {
                Description = "Live location sharing notification"
            };

            channel.SetSound(null, null);

            notificationManager?.CreateNotificationChannel(channel);
        }
    }

    public class LiveGeolocationServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public LiveGeolocationServiceConnection(LiveGeolocationServiceBinder? binder)
        {
            if (binder != null)
            {
                Binder = binder;
            }
        }

        public LiveGeolocationServiceBinder? Binder { get; private set; }

        public async void OnServiceConnected(ComponentName? name, IBinder? service)
        {
            if (!(service is LiveGeolocationServiceBinder serviceBinder))
                return;

            Binder = serviceBinder;
            Binder.IsBound = true;

            // raise the service bound event
            ServiceConnected?.Invoke(this, new ServiceConnectedEventArgs { Binder = service });

            // begin updating the location in the Service
            await serviceBinder.Service.StartLocationUpdates();
        }

        public async void OnServiceDisconnected(ComponentName? name)
        {
            if (Binder == null) return;

            Binder.IsBound = false;
            await Binder.Service.StopLocationUpdates();

            Binder = null;
        }

        public event EventHandler<ServiceConnectedEventArgs>? ServiceConnected;
    }

    public class ServiceConnectedEventArgs : EventArgs
    {
        public IBinder? Binder { get; set; }
    }
}