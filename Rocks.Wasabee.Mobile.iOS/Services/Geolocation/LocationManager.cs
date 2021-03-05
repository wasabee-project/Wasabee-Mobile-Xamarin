using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;
using Acr.UserDialogs;
using CoreLocation;
using MvvmCross;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using UIKit;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.iOS.Services.Geolocation
{
    public class LocationManager
    {
        private static readonly int MinimalUpdateTimespan = 60; // in seconds

        private WasabeeApiV1Service? _wasabeeApiV1Service;
        private ILoggingService? _loggingService;
        private IPreferences? _preferences;

        private Timer _forceSendTimer = new Timer(MinimalUpdateTimespan * 1000) { AutoReset = true };
        private bool _isRunning;
        private DateTime _lastUpdateTime;
        
        private static CultureInfo Culture => CultureInfo.GetCultureInfo("en-US");
        private static IGeolocator Geolocator => CrossGeolocator.Current;

        public CLLocationManager LocMgr { get; }
        
        public LocationManager()
        {
            LocMgr = new CLLocationManager
            {
                PausesLocationUpdatesAutomatically = false
            };

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                LocMgr.RequestAlwaysAuthorization();
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                LocMgr.AllowsBackgroundLocationUpdates = true;
            }
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

                    _forceSendTimer.Elapsed += async (sender, args) =>
                    {
                        if (!_isRunning)
                        {
                            _forceSendTimer.Stop();
                            return;
                        }

                        // Ensure it updates at least every 60 seconds
                        if (DateTime.Now - _lastUpdateTime < TimeSpan.FromSeconds(MinimalUpdateTimespan))
                            return;

                        var lastKnownLocation = await Geolocator.GetLastKnownLocationAsync();
                        if (lastKnownLocation != null)
                        {
                            try
                            {
                                var position = new Position(lastKnownLocation.Latitude, lastKnownLocation.Longitude);
                                await UpdateLocation(position);
                            }
                            catch (Exception ex)
                            {
                                _loggingService ??= Mvx.IoCProvider.Resolve<ILoggingService>();
                                _loggingService.Error(ex,
                                    "Error Executing LiveGeolocationService._forceSendTimer.Elapsed");
                            }
                        }
                    };


                    //every 5 second, 5 meters
                    await Geolocator.StartListeningAsync(TimeSpan.FromSeconds(5), 5);
                    _forceSendTimer.Start();
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
                Console.WriteLine(e);
                throw;
            }
        }
        
        private async void Geolocator_PositionChanged(object sender, PositionEventArgs e)
        {
            if (!_isRunning)
                return;
            
            _preferences ??= Mvx.IoCProvider.Resolve<IPreferences>();
            if (_preferences!.Get(UserSettingsKeys.LiveLocationSharingEnabled, false) == false)
            {
                await StopLocationUpdates();
                return;
            }

            try
            {
                _preferences.Set(UserSettingsKeys.LiveLocationSharingEnabled, true);

                var position = new Position(e.Position.Latitude, e.Position.Longitude);
                await UpdateLocation(position);
            }
            catch (Exception ex)
            {
                _loggingService ??= Mvx.IoCProvider.Resolve<ILoggingService>();
                _loggingService!.Error(ex, "Error Executing LiveGeolocationService.Geolocator_PositionChanged");
            }
        }

        public async Task StopLocationUpdates()
        {
            if (_isRunning)
            {
                _forceSendTimer.Stop();

                if (await Geolocator.StopListeningAsync())
                {
                    Geolocator.PositionChanged -= Geolocator_PositionChanged;
                    _isRunning = false;
                    
                    _preferences ??= Mvx.IoCProvider.Resolve<IPreferences>();
                    _preferences!.Set(UserSettingsKeys.LiveLocationSharingEnabled, false);
                }
            }
        }

        private async Task UpdateLocation(Position position)
        {
            var result = await _wasabeeApiV1Service!.User_UpdateLocation(position.Latitude.ToString(Culture), position.Longitude.ToString(Culture));
            if (result)
            {
                _lastUpdateTime = DateTime.Now;
            }
        }

    }
}