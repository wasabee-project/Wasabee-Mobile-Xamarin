using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.GoogleMaps;
using Rocks.Wasabee.Core.Adapters;
using Rocks.Wasabee.Core.Extensions;
using Rocks.Wasabee.Core.Helpers;
using Rocks.Wasabee.Core.Infra.Databases;
using Rocks.Wasabee.Core.Models.Agent;
using Rocks.Wasabee.Core.Models.Operations;
using Rocks.Wasabee.Core.Resources.I18n;
using Rocks.Wasabee.Core.Settings.User;
using Rocks.Wasabee.Core.ViewModels.Operation.Assignments;
using Rocks.Wasabee.Core.ViewModels.Operation.MapElements;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace Rocks.Wasabee.Core.ViewModels.Operation
{
    public partial class OperationMapViewModel : BaseViewModel
    {
        private static readonly Position DefaultPosition = new(45.50188794071243, -73.56739549536985); // Centers over Montreal, QC, Canada
        private static readonly CultureInfo ConversionCulture = CultureInfo.GetCultureInfo("en-US");

        private readonly OperationsDatabase _operationsDatabase;
        private readonly AgentsDatabase _agentsDatabase;
        private readonly IPreferencesAdapter _preferences;
        private readonly IUserSettingsService _userSettingsService;

        public OperationMapViewModel(
            OperationsDatabase operationsDatabase,
            AgentsDatabase agentsDatabase,
            IPreferencesAdapter preferences,
            IUserSettingsService userSettingsService)
        {
            _operationsDatabase = operationsDatabase;
            _agentsDatabase = agentsDatabase;
            _preferences = preferences;
            _userSettingsService = userSettingsService;
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            LoadOperationCommand.ExecuteAsync(null);
        }

        #region Properties

        [ObservableProperty] public partial bool IsLoading { get; set; }

        [ObservableProperty] public partial OperationModel? Operation { get; set; }
        [ObservableProperty] public partial WasabeePin? SelectedWasabeePin { get; set; }
        [ObservableProperty] public partial WasabeeAgentPin? SelectedAgentPin { get; set; }

        [ObservableProperty] public partial ObservableCollection<WasabeeLink> Links { get; set; } = [];
        [ObservableProperty] public partial ObservableCollection<WasabeePin> Anchors { get; set; } = [];
        [ObservableProperty] public partial ObservableCollection<WasabeePin> Markers { get; set; } = [];
        [ObservableProperty] public partial ObservableCollection<WasabeeAgentPin> Agents { get; set; } = [];
        [ObservableProperty] public partial ObservableCollection<WasabeeOperationZone> Zones { get; set; } = [];

        [ObservableProperty] public partial MapSpan OperationMapRegion { get; set; } = MapSpan.FromCenterAndRadius(DefaultPosition, Distance.FromKilometers(5));

        [ObservableProperty] public partial MapSpan? VisibleRegion { get; set; }

        [ObservableProperty] public partial bool IsLocationAvailable { get; set; } = false;

        [ObservableProperty] public partial MapThemeEnum MapTheme { get; set; } = MapThemeEnum.GoogleLight;
        [ObservableProperty] public partial MapType MapType { get; set; } = MapType.Street;
        [ObservableProperty] public partial bool IsStylingAvailable { get; set; } = true;

        [ObservableProperty] public partial bool IsLayerChooserVisible { get; set; } = false;

        [ObservableProperty] public partial bool IsMarkerDetailAvailable { get; set; }

        [ObservableProperty] public partial bool IsAgentListVisible { get; set; }

        public Pin? SelectedPin
        {
            get => SelectedWasabeePin?.Pin;
            set
            {
                SelectedWasabeePin = value != null
                    ? Anchors.FirstOrDefault(x => x.Pin == value) ?? Markers.FirstOrDefault(x => x.Pin == value)
                    : null;
                OnPropertyChanged();

                IsMarkerDetailAvailable = !string.IsNullOrWhiteSpace(SelectedWasabeePin?.Marker.Id);
            }
        }

        private bool _isLayerLinksActivated = true;
        public bool IsLayerLinksActivated
        {
            get => _isLayerLinksActivated;
            set
            {
                SetProperty(ref _isLayerLinksActivated, value);
                UpdateMapLayersConfig();
            }
        }

        private bool _isLayerMarkersActivated = true;
        public bool IsLayerMarkersActivated
        {
            get => _isLayerMarkersActivated;
            set
            {
                SetProperty(ref _isLayerMarkersActivated, value);
                UpdateMapLayersConfig();
            }
        }

        private bool _isLayerAnchorsActivated = true;
        public bool IsLayerAnchorsActivated
        {
            get => _isLayerAnchorsActivated;
            set
            {
                SetProperty(ref _isLayerAnchorsActivated, value);
                UpdateMapLayersConfig();
            }
        }

        private bool _isLayerAgentsActivated = true;
        public bool IsLayerAgentsActivated
        {
            get => _isLayerAgentsActivated;
            set
            {
                SetProperty(ref _isLayerAgentsActivated, value);
                UpdateMapLayersConfig();
            }
        }

        private bool _isLayerZonesActivated = true;
        public bool IsLayerZonesActivated
        {
            get => _isLayerZonesActivated;
            set
            {
                SetProperty(ref _isLayerZonesActivated, value);
                UpdateMapLayersConfig();
            }
        }


        #endregion

        #region Commands

        [RelayCommand]
        public async Task LoadOperationAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedOpId))
                return;

            Links.Clear();
            Anchors.Clear();
            Markers.Clear();
            Zones.Clear();

            Operation = await _operationsDatabase.GetOperationModel(selectedOpId);
            if (Operation == null)
            {
                IsLoading = false;
                return;
            }

            try
            {
                foreach (var link in Operation.Links)
                {
                    try
                    {
                        var newLink = CreateWasabeeLink(link);
                        if (newLink is null)
                            return;

                        Links.Add(newLink);
                    }
                    catch (Exception e)
                    {
                    }
                }

                foreach (var anchorId in Operation.Anchors)
                {
                    try
                    {
                        var portal = Operation.Portals.FirstOrDefault(p => p.Id.Equals(anchorId));
                        if (portal == null)
                            continue;

                        double.TryParse(portal.Lat, NumberStyles.Float, ConversionCulture, out var portalLat);
                        double.TryParse(portal.Lng, NumberStyles.Float, ConversionCulture, out var portalLng);

                        Anchors.Add(new WasabeePin(new Pin
                        {
                            Position = new Position(portalLat, portalLng),
                            Icon = BitmapDescriptorFactory.FromBundle($"pin_{Operation.Color}"),
                            Anchor = new Point(0.5, 1)
                        })
                        {
                            Portal = portal
                        });
                    }
                    catch (Exception e)
                    {
                    }
                }

                var hideCompletedMarkersSetting = _preferences.Get(UserSettingsKeys.HideCompletedMarkers, false);
                foreach (var marker in Operation.Markers)
                {
                    try
                    {
                        if (hideCompletedMarkersSetting && marker.State is TaskState.Completed)
                            continue;

                        var portal = Operation.Portals.First(x => x.Id.Equals(marker.PortalId));
                        double.TryParse(portal.Lat, NumberStyles.Float, ConversionCulture, out var portalLat);
                        double.TryParse(portal.Lng, NumberStyles.Float, ConversionCulture, out var portalLng);

                        var pin = new WasabeePin(
                            new Pin
                            {
                                Position = new Position(portalLat, portalLng),
                                Icon = BitmapDescriptorFactory.FromBundle($"{marker.Type}|{marker.State}"),
                                Label = GetMarkerNameFromTypeAndState(marker.Type, marker.State),
                                Anchor = new Point(0.5, 1)
                            })
                        {
                            Portal = portal,
                            Marker = marker,
                        };

                        if (marker.Assignments.IsNotNullOrEmpty())
                            pin.Assignments = string.Join(", ", _agentsDatabase.GetAgents(marker.Assignments).Result
                                .Select(x => x.Name).OrderBy(x => x));

                        Markers.Add(pin);
                    }
                    catch (Exception e)
                    {
                    }
                }

                foreach (var zone in Operation.Zones.Where(z => z.Points.IsNotNullOrEmpty()))
                {
                    try
                    {
                        var wZone = new WasabeeOperationZone(zone.Name)
                        {
                            Color = WasabeeColorsHelper.GetColorFromWasabeeName(zone.Color, string.Empty)
                        };

                        if (BuildZonePolygon(wZone.Polygon, zone.Points.OrderBy(z => z.Position)))
                            Zones.Add(wZone);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
            catch (NullReferenceException e)
            {
            }
            finally
            {
                OnPropertyChanged(nameof(Links));
                OnPropertyChanged(nameof(Anchors));
                OnPropertyChanged(nameof(Markers));
                OnPropertyChanged(nameof(Zones));

                UpdateMapRegion();

                //_messenger.Publish(new MessageFrom<MapViewModel>(this, forceResetMapView));
                IsLoading = false;
            }
        }

        #endregion

        #region Private methods

        private MarkerAssignmentData CreateMarkerAssignmentData(MarkerModel marker)
        {
            if (Operation == null)
                throw new Exception("Operation is null");

            return new MarkerAssignmentData(Operation.Id, marker.Order)
            {
                Marker = marker,
                Assignments = marker.Assignments.IsNullOrEmpty() ? string.Empty :
                    string.Join(", ", _agentsDatabase.GetAgents(marker.Assignments).Result.Select(x => x.Name).OrderBy(x => x)),
                Portal = Operation.Portals?.FirstOrDefault(p => p.Id.Equals(marker.PortalId))
            };
        }

        private async Task<bool> CheckAndAskForLocationPermissions()
        {
            var statusLocationAlways = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (statusLocationAlways == PermissionStatus.Granted)
                return true;

            var requestPermission = true;
            var showRationale = Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>();
            if (showRationale)
            {
                //requestPermission = await _userDialogs.ConfirmAsync(
                //    Strings.Dialogs_Warning_PermissionWhenInUse,
                //    Strings.Dialog_Warning_PermissionsRequired,
                //    Strings.Global_Ok,
                //    Strings.Global_Cancel);
            }

            if (!requestPermission)
                return false;

            statusLocationAlways = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (statusLocationAlways != PermissionStatus.Granted)
            {
                //_userDialogs.Alert(Strings.Dialogs_Warning_LocationPermissionRequired);
                return false;
            }

            return true;
        }

        private WasabeeLink? CreateWasabeeLink(LinkModel linkModel)
        {
            if (Operation is null)
                return null;

            var culture = CultureInfo.GetCultureInfo("en-US");

            var fromPortal = Operation.Portals.FirstOrDefault(x => x.Id.Equals(linkModel.FromPortalId));
            var toPortal = Operation.Portals.FirstOrDefault(x => x.Id.Equals(linkModel.ToPortalId));

            if (fromPortal == null || toPortal == null)
                return null;

            double.TryParse(fromPortal.Lat, NumberStyles.Float, culture, out var fromLat);
            double.TryParse(fromPortal.Lng, NumberStyles.Float, culture, out var fromLng);
            double.TryParse(toPortal.Lat, NumberStyles.Float, culture, out var toLat);
            double.TryParse(toPortal.Lng, NumberStyles.Float, culture, out var toLng);

            var baseLinkColor = WasabeeColorsHelper.GetColorFromWasabeeName(linkModel.Color, Operation.Color);
            if (linkModel.State is TaskState.Completed)
                baseLinkColor = baseLinkColor.MultiplyAlpha(0.5f);

            var wasabeeLink = new WasabeeLink(
                new Polyline
                {
                    IsGeodesic = true,
                    StrokeColor = baseLinkColor,
                    StrokeWidth = linkModel.State is TaskState.Completed ? 1 : 2,
                    Positions =
                    {
                        new Position(fromLat, fromLng),
                        new Position(toLat, toLng)
                    }
                }
            )
            {
                LinkId = linkModel.Id
            };

            return wasabeeLink;
        }

        private WasabeeAgentPin CreateAgentPin(AgentModel agent, bool isAgentAssignedToOperation, bool isCurrentUser = false)
        {
            var pin = new Pin
            {
                Label = agent.Name,
                Position = new Position(agent.Lat, agent.Lng),
                Icon = BitmapDescriptorFactory.FromBundle(isCurrentUser ? "wasabee_player_marker_self" :
                    isAgentAssignedToOperation ? "wasabee_player_marker" : "wasabee_player_marker_gray")
            };
            var playerPin = new WasabeeAgentPin(pin) { AgentId = agent.Id, AgentName = agent.Name };

            if (!string.IsNullOrWhiteSpace(agent.Date) && DateTime.TryParse(agent.Date, out var agentDate))
            {
                var timeAgo = (DateTime.UtcNow - agentDate);
                if (timeAgo.TotalSeconds <= 1.0)
                    return playerPin;

                playerPin.TimeAgo = timeAgo.ToPrettyString();
                if (string.IsNullOrEmpty(playerPin.TimeAgo) is false)
                    playerPin.Pin.Label += $" - {string.Format(Strings.Map_Text_TimeAgo, playerPin.TimeAgo)}";
            }

            return playerPin;
        }

        private static bool BuildZonePolygon(Polygon polygonToBuild, IEnumerable<ZonePointModel> points)
        {
            try
            {
                foreach (var point in points)
                {
                    polygonToBuild.Positions.Add(new Position(point.Lat, point.Lng));
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void UpdateMapRegion()
        {
            var hasValues = false;

            var lowestLat = 0.0d;
            var highestLat = 0.0d;
            var lowestLong = 0.0d;
            var highestLong = 0.0d;

            if (Markers.Any())
            {
                lowestLat = Markers.Min(p => p.Pin.Position.Latitude);
                highestLat = Markers.Max(p => p.Pin.Position.Latitude);
                lowestLong = Markers.Min(p => p.Pin.Position.Longitude);
                highestLong = Markers.Max(p => p.Pin.Position.Longitude);

                hasValues = true;
            }
            if (Anchors.Any())
            {
                lowestLat = hasValues ? Math.Min(lowestLat, Anchors.Min(p => p.Pin.Position.Latitude)) : Anchors.Min(p => p.Pin.Position.Latitude);
                highestLat = hasValues ? Math.Max(highestLat, Anchors.Max(p => p.Pin.Position.Latitude)) : Anchors.Max(p => p.Pin.Position.Latitude);
                lowestLong = hasValues ? Math.Min(lowestLong, Anchors.Min(p => p.Pin.Position.Longitude)) : Anchors.Min(p => p.Pin.Position.Longitude);
                highestLong = hasValues ? Math.Max(highestLong, Anchors.Max(p => p.Pin.Position.Longitude)) : Anchors.Max(p => p.Pin.Position.Longitude);

                hasValues = true;
            }
            if (Zones.Any())
            {
                var zonesLowestLat = Zones.Select(x => x.Polygon.Positions.Min(y => y.Latitude)).Min();
                var zonesHighestLat = Zones.Select(x => x.Polygon.Positions.Max(y => y.Latitude)).Max();
                var zonesLowestLng = Zones.Select(x => x.Polygon.Positions.Min(y => y.Longitude)).Min();
                var zonesHighestLng = Zones.Select(x => x.Polygon.Positions.Max(y => y.Longitude)).Max();

                lowestLat = hasValues ? Math.Min(lowestLat, zonesLowestLat) : zonesLowestLat;
                highestLat = hasValues ? Math.Max(highestLat, zonesHighestLat) : zonesHighestLat;
                lowestLong = hasValues ? Math.Min(lowestLong, zonesLowestLng) : zonesLowestLng;
                highestLong = hasValues ? Math.Max(highestLong, zonesHighestLng) : zonesHighestLng;
            }

            var finalLat = (lowestLat + highestLat) / 2;
            var finalLong = (lowestLong + highestLong) / 2;
            var distance = DistanceCalculation.GeoCodeCalc.CalcDistance(lowestLat, lowestLong, highestLat,
                highestLong, DistanceCalculation.GeoCodeCalcMeasurement.Kilometers);

            if (distance != 0.0d)
                OperationMapRegion = MapSpan.FromCenterAndRadius(new Position(finalLat, finalLong), Distance.FromKilometers(distance));
            else
                OperationMapRegion = MapSpan.FromCenterAndRadius(DefaultPosition, Distance.FromKilometers(5000));
        }

        //private void UpdateLink(LinkDataChangedMessage updateMessage)
        //{
        //    if (Operation == null)
        //        return;

        //    if (!updateMessage.OperationId.Equals(Operation.Id))
        //        return;

        //    var oldLink = Links.FirstOrDefault(x => x.LinkId.Equals(updateMessage.LinkData.Id));
        //    if (oldLink != null)
        //    {
        //        var newLink = CreateWasabeeLink(updateMessage.LinkData);
        //        if (newLink is null)
        //            return;

        //        Links.Remove(oldLink);
        //        Links.Add(newLink);

        //        //_messenger.Publish(new MessageFrom<MapViewModel>(this, false));

        //        // If assigned to current user
        //        if (updateMessage.LinkData.Assignments.IsNotNullOrEmpty() &&
        //            updateMessage.LinkData.Assignments.Contains(_userSettingsService.GetLoggedUserGoogleId()))
        //        {
        //            //_messenger.Publish(new MessageFor<AssignmentsListViewModel>(this));
        //        }
        //    }
        //}

        //private void UpdateMarker(MarkerDataChangedMessage updateMessage)
        //{
        //    if (Operation == null)
        //        return;

        //    if (!updateMessage.OperationId.Equals(Operation.Id))
        //        return;

        //    var marker = Markers.FirstOrDefault(x => x.Marker.Id.Equals(updateMessage.MarkerData.Id));
        //    if (marker != null)
        //    {
        //        Markers.Remove(marker);

        //        var hideCompletedMarkersSetting = _preferences.Get(UserSettingsKeys.HideCompletedMarkers, false);
        //        if (hideCompletedMarkersSetting && updateMessage.MarkerData.State is TaskState.Completed)
        //        {
        //            CloseDetailPanelCommand.Execute();
        //        }
        //        else
        //        {
        //            var culture = CultureInfo.GetCultureInfo("en-US");
        //            var portal = Operation.Portals.First(x => x.Id.Equals(updateMessage.MarkerData.PortalId));
        //            double.TryParse(portal.Lat, NumberStyles.Float, culture, out var portalLat);
        //            double.TryParse(portal.Lng, NumberStyles.Float, culture, out var portalLng);

        //            var pin = new WasabeePin(
        //                new Pin
        //                {
        //                    Position = new Position(portalLat, portalLng),
        //                    Icon = BitmapDescriptorFactory.FromBundle(
        //                        $"{updateMessage.MarkerData.Type}|{updateMessage.MarkerData.State}"),
        //                    Label = GetMarkerNameFromTypeAndState(updateMessage.MarkerData.Type,
        //                        updateMessage.MarkerData.State)
        //                })
        //            {
        //                Portal = portal,
        //                Marker = updateMessage.MarkerData
        //            };

        //            if (updateMessage.MarkerData.Assignments.IsNotNullOrEmpty())
        //                pin.Assignments = string.Join(", ", _agentsDatabase.GetAgents(updateMessage.MarkerData.Assignments).Result
        //                    .Select(x => x.Name).OrderBy(x => x));

        //            Markers.Add(pin);
        //            SelectedWasabeePin = pin;
        //        }

        //        //_messenger.Publish(new MessageFrom<MapViewModel>(this, false));

        //        // If assigned to current user
        //        if (updateMessage.MarkerData.Assignments.IsNotNullOrEmpty() &&
        //            updateMessage.MarkerData.Assignments.Contains(_userSettingsService.GetLoggedUserGoogleId()))
        //        {
        //            //_messenger.Publish(new MessageFor<AssignmentsListViewModel>(this));
        //        }
        //    }
        //}

        private string GetMarkerNameFromTypeAndState(MarkerType markerType, TaskState markerState)
        {
            return $"{markerType.ToFriendlyString()} - {markerState.ToFriendlyString()}";
        }

        private readonly Lock _mapLayerConfigLock = new();
        private void UpdateMapLayersConfig()
        {
            lock (_mapLayerConfigLock)
            {
                var config = new MapLayersConfig
                {
                    Links = IsLayerLinksActivated,
                    Markers = IsLayerMarkersActivated,
                    Anchors = IsLayerAnchorsActivated,
                    Agents = IsLayerAgentsActivated,
                    Zones = IsLayerZonesActivated
                };

                var raw = JsonSerializer.Serialize(config);
                _preferences.Set(UserSettingsKeys.MapLayers, raw);
            }
        }

        #endregion
    }

    public enum MapThemeEnum
    {
        GoogleLight,
        Enlightened,
        IntelDefault,
        RedIntel
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
                return CalcDistance(lat1, lng1, lat2, lng2, GeoCodeCalcMeasurement.Kilometers);
            }

            public static double CalcDistance(double lat1, double lng1, double lat2, double lng2, GeoCodeCalcMeasurement m)
            {
                var radius = m switch
                {
                    GeoCodeCalcMeasurement.Miles => EarthRadiusInMiles,
                    GeoCodeCalcMeasurement.Kilometers => EarthRadiusInKilometers,
                    _ => throw new ArgumentOutOfRangeException(nameof(m), m, null)
                };

                return radius * Math.Asin(Math.Min(1, Math.Sqrt((Math.Pow(Math.Sin((DiffRadian(lat1, lat2)) / 2.0), 2.0) + Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * Math.Pow(Math.Sin((DiffRadian(lng1, lng2)) / 2.0), 2.0)))));
            }
        }

        public enum GeoCodeCalcMeasurement
        {
            Miles = 0,
            Kilometers = 1
        }
    }

    internal class MapLayersConfig
    {
        public bool Links { get; set; }
        public bool Markers { get; set; }
        public bool Anchors { get; set; }
        public bool Agents { get; set; }
        public bool Zones { get; set; }
    }
}