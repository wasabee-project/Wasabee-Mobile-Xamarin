using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms.GoogleMaps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Map
{
    public class MapViewModel : BaseViewModel
    {
        private static readonly Position DefaultPosition = new Position(45.767723, 4.835711); // Centers over Lyon, France

        private readonly OperationsDatabase _operationsDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly IPreferences _preferences;
        private readonly IPermissions _permissions;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigationService;
        private readonly IUserSettingsService _userSettingsService;

        private readonly MvxSubscriptionToken _token;

        public MapViewModel(OperationsDatabase operationsDatabase, TeamsDatabase teamsDatabase, IPreferences preferences,
            IPermissions permissions, IMvxMessenger messenger, IUserDialogs userDialogs, IMvxNavigationService navigationService,
            IUserSettingsService userSettingsService)
        {
            _operationsDatabase = operationsDatabase;
            _teamsDatabase = teamsDatabase;
            _preferences = preferences;
            _permissions = permissions;
            _userDialogs = userDialogs;
            _navigationService = navigationService;
            _userSettingsService = userSettingsService;

            _token = messenger.Subscribe<SelectedOpChangedMessage>(async msg => await LoadOperationCommand.ExecuteAsync());
        }

        public override async void Prepare()
        {
            base.Prepare();

            var statusLocationAlways = await _permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (statusLocationAlways != PermissionStatus.Granted)
            {
                var result = await _permissions.RequestAsync<Permissions.LocationAlways>();
                if (result != PermissionStatus.Granted)
                {
                    var ifInUsePermission = await _permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                    if (ifInUsePermission != PermissionStatus.Granted)
                        _userDialogs.Alert("Geolocation permission is required to show your position !");
                    else
                    {
                        LoggingService.Info("User has granted WhenInUse geolocation permissions");
                        IsLocationAvailable = true;
                    }
                }
                else
                {
                    LoggingService.Info("User has granted full geolocation permissions");
                    IsLocationAvailable = true;
                }
            }
            else
            {
                IsLocationAvailable = true;
            }
        }

        public override async Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);
            LoggingService.Trace("Navigated to MapViewModel");

            await base.Initialize();
            await LoadOperationCommand.ExecuteAsync();
        }

        #region Properties

        public Pin SelectedPin
        {
            get => SelectedWasabeePin?.Pin;
            set
            {
                SelectedWasabeePin = value != null ? Pins.FirstOrDefault(x => x.Pin == value) : null;
                RaisePropertyChanged(() => SelectedPin);
            }
        }

        public OperationModel Operation { get; set; }
        public WasabeePin SelectedWasabeePin { get; set; } = null;

        public MvxObservableCollection<Polyline> Polylines { get; set; } = new MvxObservableCollection<Polyline>();
        public MvxObservableCollection<WasabeePin> Pins { get; set; } = new MvxObservableCollection<WasabeePin>();
        public MapSpan OperationMapRegion { get; set; } = MapSpan.FromCenterAndRadius(DefaultPosition, Distance.FromKilometers(5));

        public MapSpan VisibleRegion { get; set; }

        public bool IsLocationAvailable { get; set; } = false;

        #endregion

        #region Commands

        public IMvxCommand CloseDetailPanelCommand => new MvxCommand(() => SelectedPin = null);

        public IMvxCommand MoveToPortalCommand => new MvxCommand(MoveToPortalExecuted);
        private void MoveToPortalExecuted()
        {
            LoggingService.Trace("Executing MapViewModel.MoveToPortalCommand");

            if (SelectedWasabeePin == null)
                return;

            var region = MapSpan.FromCenterAndRadius(SelectedWasabeePin.Pin.Position, Distance.FromMeters(200));
            if (VisibleRegion == region)
                RaisePropertyChanged(() => VisibleRegion);
            else
                VisibleRegion = region;
        }

        public IMvxAsyncCommand LoadOperationCommand => new MvxAsyncCommand(LoadOperationExecuted);
        private async Task LoadOperationExecuted()
        {
            if (IsBusy) return;
            IsBusy = true;

            LoggingService.Trace("Executing MapViewModel.LoadOperationCommand");

            var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedOpId))
                return;

            Polylines.Clear();
            Pins.Clear();

            Operation = await _operationsDatabase.GetOperationModel(selectedOpId);
            var teamsAgentsLists = (await _teamsDatabase.GetTeams(_userSettingsService.GetLoggedUserGoogleId()))
                ?.SelectMany(t => t.Agents).Select(a => new { a.Id, a.Name }).Distinct().ToList();
            try
            {
                var culture = CultureInfo.GetCultureInfo("en-US");
                foreach (var link in Operation.Links)
                {
                    var fromPortal = Operation.Portals.First(x => x.Id.Equals(link.FromPortalId));
                    var toPortal = Operation.Portals.First(x => x.Id.Equals(link.ToPortalId));

                    try
                    {
                        double.TryParse(fromPortal.Lat, NumberStyles.Float, culture, out var fromLat);
                        double.TryParse(fromPortal.Lng, NumberStyles.Float, culture, out var fromLng);
                        double.TryParse(toPortal.Lat, NumberStyles.Float, culture, out var toLat);
                        double.TryParse(toPortal.Lng, NumberStyles.Float, culture, out var toLng);

                        Polylines.Add(
                            new Polyline()
                            {
                                StrokeColor = WasabeeColorsHelper.GetColorFromWasabeeName(link.Color),
                                StrokeWidth = 2,
                                Positions =
                                {
                                    new Position(fromLat, fromLng),
                                    new Position(toLat, toLng)
                                }
                            });

                        Pins.Add(new WasabeePin(new Pin()
                        {
                            Position = new Position(fromLat, fromLng),
                            Icon = BitmapDescriptorFactory.FromBundle($"marker_layer_{Operation.Color}")
                        })
                        {
                            Portal = fromPortal
                        });
                        Pins.Add(new WasabeePin(new Pin()
                        {
                            Position = new Position(toLat, toLng),
                            Icon = BitmapDescriptorFactory.FromBundle($"marker_layer_{Operation.Color}")
                        })
                        {
                            Portal = toPortal
                        });
                    }
                    catch (Exception e)
                    {
                        LoggingService.Error("Error Executing MapViewModel.LoadOperationCommand", e);
                    }
                }

                foreach (var marker in Operation.Markers)
                {
                    try
                    {
                        var portal = Operation.Portals.First(x => x.Id.Equals(marker.PortalId));
                        double.TryParse(portal.Lat, NumberStyles.Float, culture, out var portalLat);
                        double.TryParse(portal.Lng, NumberStyles.Float, culture, out var portalLng);

                        var pin = new WasabeePin(
                            new Pin()
                            {
                                Position = new Position(portalLat, portalLng),
                                Icon = BitmapDescriptorFactory.FromBundle($"{marker.Type}|{marker.State}")
                            })
                        {
                            Portal = portal,
                            Marker = marker
                        };

                        if (!teamsAgentsLists.IsNullOrEmpty())
                            pin.AssignedTo = teamsAgentsLists!.FirstOrDefault(a => a.Id.Equals(marker.AssignedTo))
                                ?.Name;

                        Pins.Add(pin);
                    }
                    catch (Exception e)
                    {
                        LoggingService.Error("Error Executing MapViewModel.LoadOperationCommand", e);
                    }
                }
            }
            catch (NullReferenceException e)
            {
                LoggingService.Error("Error Executing MapViewModel.LoadOperationCommand", e);
            }
            finally
            {
                if (Pins.Any())
                {
                    var lowestLat = Pins.Min(p => p.Pin.Position.Latitude);
                    var highestLat = Pins.Max(p => p.Pin.Position.Latitude);
                    var lowestLong = Pins.Min(p => p.Pin.Position.Longitude);
                    var highestLong = Pins.Max(p => p.Pin.Position.Longitude);
                    var finalLat = (lowestLat + highestLat) / 2;
                    var finalLong = (lowestLong + highestLong) / 2;
                    var distance = DistanceCalculation.GeoCodeCalc.CalcDistance(lowestLat, lowestLong, highestLat,
                        highestLong, DistanceCalculation.GeoCodeCalcMeasurement.Kilometers);

                    OperationMapRegion = MapSpan.FromCenterAndRadius(new Position(finalLat, finalLong), Distance.FromKilometers(distance));
                }

                IsBusy = false;
            }
        }

        #endregion
    }

    internal class DistanceCalculation
    {
        public static class GeoCodeCalc
        {
            private const double EarthRadiusInMiles = 3956.0;
            private const double EarthRadiusInKilometers = 6367.0;

            public static double ToRadian(double val) { return val * (Math.PI / 180); }
            public static double DiffRadian(double val1, double val2) { return ToRadian(val2) - ToRadian(val1); }

            public static double CalcDistance(double lat1, double lng1, double lat2, double lng2)
            {
                return CalcDistance(lat1, lng1, lat2, lng2, GeoCodeCalcMeasurement.Miles);
            }

            public static double CalcDistance(double lat1, double lng1, double lat2, double lng2, GeoCodeCalcMeasurement m)
            {
                var radius = m switch
                {
                    GeoCodeCalcMeasurement.Miles => EarthRadiusInMiles,
                    GeoCodeCalcMeasurement.Kilometers => EarthRadiusInKilometers,
                    _ => EarthRadiusInKilometers
                };

                return radius * Math.Asin(Math.Min(1, Math.Sqrt((Math.Pow(Math.Sin((DiffRadian(lat1, lat2)) / 2.0), 2.0) + Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * Math.Pow(Math.Sin((DiffRadian(lng1, lng2)) / 2.0), 2.0)))));
            }
        }

        public enum GeoCodeCalcMeasurement : int
        {
            Miles = 0,
            Kilometers = 1
        }
    }
}