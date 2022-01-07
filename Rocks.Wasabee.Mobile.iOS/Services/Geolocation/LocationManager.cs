using Acr.UserDialogs;
using CoreLocation;
using MvvmCross;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Rocks.Wasabee.Mobile.Core.Helpers.Xaml;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.iOS.Services.Geolocation
{
    public class LocationManager
    {
        private static readonly int MinimalUpdateTimespan = 60; // in seconds

        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly ILoggingService _loggingService;
        private readonly IPreferences _preferences;
        private readonly IPermissions _permissions;
        private readonly IUserDialogs _userDialogs;

        private readonly Timer _forceSendTimer = new Timer(MinimalUpdateTimespan * 1000) { AutoReset = true };

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

            _wasabeeApiV1Service = Mvx.IoCProvider.Resolve<WasabeeApiV1Service>();
            _preferences = Mvx.IoCProvider.Resolve<IPreferences>();
            _loggingService = Mvx.IoCProvider.Resolve<ILoggingService>();
            _permissions = Mvx.IoCProvider.Resolve<IPermissions>();
            _userDialogs = Mvx.IoCProvider.Resolve<IUserDialogs>();

            _loggingService.Trace("LocationManager Initialized");
        }

        public async Task StartLocationUpdates()
        {
            if (_isRunning)
                return;

            if (await EnsureHasPermissions() is false)
            {
                _loggingService.Trace("Can't Execute LocationManager.StartLocationUpdates, no permissions");

                if (_preferences.Get(UserSettingsKeys.ShowDebugToasts, false) is true)
                    _userDialogs.Toast(TranslateExtension.GetValue("Toast_CantStartLiveLocSharing"));

                return;
            }

            _loggingService.Trace("Executing LocationManager.StartLocationUpdates");

            try
            {
                if (_preferences.Get(UserSettingsKeys.ShowDebugToasts, false) is true)
                    _userDialogs.Toast(TranslateExtension.GetValue("Toast_LiveLocSharingStarted"));

                _isRunning = true;

                if (Geolocator.IsGeolocationAvailable && Geolocator.IsGeolocationEnabled)
                {
                    if (CLLocationManager.LocationServicesEnabled)
                    {
                        LocMgr.DesiredAccuracy = 5;
                        LocMgr.LocationsUpdated += LocMgr_LocationsUpdated;

                        LocMgr.StartUpdatingLocation();
                    }

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
                                _loggingService.Error(ex, "Error Executing LocationManager._forceSendTimer.Elapsed");
                            }
                        }
                    };

                    _forceSendTimer.Start();
                }
                else
                {
                    Mvx.IoCProvider.Resolve<IUserDialogs>().Alert(
                        TranslateExtension.GetValue("Dialog_Warning_GeolocationDisabled-Text"),
                        TranslateExtension.GetValue("Dialog_Warning_GeolocationDisabled"),
                        TranslateExtension.GetValue("Global_Ok"));
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error Executing LocationManager.StartLocationUpdates");
            }
        }

        private async Task<bool> EnsureHasPermissions()
        {
            _loggingService.Trace("Executing LocationManager.EnsureHasPermissions");

            var locAlwaysPermission = await _permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (locAlwaysPermission != PermissionStatus.Granted)
                locAlwaysPermission = await _permissions.RequestAsync<Permissions.LocationAlways>();

            if (locAlwaysPermission != PermissionStatus.Granted)
                return false;

            return true;
        }

        private async void LocMgr_LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            if (!_isRunning)
                return;

            _loggingService.Trace("Executing LocationManager.LocMgr_LocationsUpdated");

            if (_preferences.Get(UserSettingsKeys.LiveLocationSharingEnabled, false) == false)
            {
                StopLocationUpdates();
                return;
            }

            try
            {
                // Prevent server spam, location updates interval is at least 15 seconds when moving
                if (DateTime.Now - _lastUpdateTime < TimeSpan.FromSeconds(15))
                    return;

                _preferences.Set(UserSettingsKeys.LiveLocationSharingEnabled, true);

                var lastLocation = e.Locations[e.Locations.Length - 1];
                var position = new Position(lastLocation.Coordinate.Latitude, lastLocation.Coordinate.Longitude);
                await UpdateLocation(position);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex, "Error Executing LocationManager.LocMgr_LocationsUpdated");
            }
        }

        public void StopLocationUpdates()
        {
            if (_isRunning)
            {
                _forceSendTimer.Stop();

                LocMgr.StopUpdatingLocation();
                LocMgr.LocationsUpdated -= LocMgr_LocationsUpdated;
                _isRunning = false;

                _preferences.Set(UserSettingsKeys.LiveLocationSharingEnabled, false);
                _loggingService.Trace("Executing LocationManager.StopLocationUpdates");
            }
        }

        private async Task UpdateLocation(Position position)
        {
            _loggingService.Trace("Executing LocationManager.UpdateLocation");
            var result = await _wasabeeApiV1Service!.User_UpdateLocation(position.Latitude.ToString(Culture), position.Longitude.ToString(Culture));
            if (result)
            {
                _lastUpdateTime = DateTime.Now;

                if (_preferences.Get(UserSettingsKeys.ShowDebugToasts, false) is true)
                    _userDialogs.Toast(TranslateExtension.GetValue("Toast_LiveLocPositionUpdated"), TimeSpan.FromMilliseconds(750));
            }
            else
                _loggingService.Trace("Failed Executing LocationManager.UpdateLocation");
        }
    }
}