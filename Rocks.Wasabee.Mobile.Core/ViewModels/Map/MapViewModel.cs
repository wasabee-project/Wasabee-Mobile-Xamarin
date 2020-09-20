using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using MoreLinq;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Map
{
    public class MapViewModel : BaseViewModel
    {
        private static readonly Position DefaultPosition = new Position(45.767723, 4.835711); // Centers over Lyon, France
        private static readonly double PositionOffset = 0.0000005f;

        private readonly OperationsDatabase _operationsDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly UsersDatabase _usersDatabase;
        private readonly IPreferences _preferences;
        private readonly IPermissions _permissions;
        private readonly IMvxMessenger _messenger;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxNavigationService _navigationService;
        private readonly IUserSettingsService _userSettingsService;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;

        private readonly MvxSubscriptionToken _token;
        private readonly MvxSubscriptionToken _tokenLiveLocation;

        private bool _loadingAgentsLocations;

        public MapViewModel(OperationsDatabase operationsDatabase, TeamsDatabase teamsDatabase, UsersDatabase usersDatabase, IPreferences preferences,
            IPermissions permissions, IMvxMessenger messenger, IUserDialogs userDialogs, IMvxNavigationService navigationService,
            IUserSettingsService userSettingsService, WasabeeApiV1Service wasabeeApiV1Service)
        {
            _operationsDatabase = operationsDatabase;
            _teamsDatabase = teamsDatabase;
            _usersDatabase = usersDatabase;
            _preferences = preferences;
            _permissions = permissions;
            _messenger = messenger;
            _userDialogs = userDialogs;
            _navigationService = navigationService;
            _userSettingsService = userSettingsService;
            _wasabeeApiV1Service = wasabeeApiV1Service;

            _token = messenger.Subscribe<SelectedOpChangedMessage>(async msg => await LoadOperationCommand.ExecuteAsync());
            _tokenLiveLocation = messenger.Subscribe<TeamAgentLocationUpdatedMessage>(async msg => await RefreshTeamsMembersPositionsCommand.ExecuteAsync());
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

            MapTheme = _preferences.Get(UserSettingsKeys.MapTheme, nameof(MapThemeEnum.Light)) switch
            {
                nameof(MapThemeEnum.Dark) => MapThemeEnum.Dark,
                _ => MapThemeEnum.Light
            };

            await base.Initialize();

            await LoadOperationCommand.ExecuteAsync();
            await RefreshTeamsMembersPositionsCommand.ExecuteAsync();
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
        public MvxObservableCollection<WasabeePlayerPin> AgentsPins { get; set; } = new MvxObservableCollection<WasabeePlayerPin>();

        public MapSpan OperationMapRegion { get; set; } = MapSpan.FromCenterAndRadius(DefaultPosition, Distance.FromKilometers(5));

        public MapSpan VisibleRegion { get; set; }

        public bool IsLocationAvailable { get; set; } = false;

        public MapThemeEnum MapTheme { get; set; }

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

                    try
                    {
                        var fromPortal = Operation.Portals.FirstOrDefault(x => x.Id.Equals(link.FromPortalId));
                        var toPortal = Operation.Portals.FirstOrDefault(x => x.Id.Equals(link.ToPortalId));

                        if (fromPortal == null || toPortal == null)
                            continue;

                        double.TryParse(fromPortal.Lat, NumberStyles.Float, culture, out var fromLat);
                        double.TryParse(fromPortal.Lng, NumberStyles.Float, culture, out var fromLng);
                        double.TryParse(toPortal.Lat, NumberStyles.Float, culture, out var toLat);
                        double.TryParse(toPortal.Lng, NumberStyles.Float, culture, out var toLng);

                        Polylines.Add(
                            new Polyline()
                            {
                                StrokeColor = WasabeeColorsHelper.GetColorFromWasabeeName(link.Color, Operation.Color),
                                StrokeWidth = 2,
                                Positions =
                                {
                                    new Position(fromLat, fromLng),
                                    new Position(toLat, toLng)
                                }
                            });

                    }
                    catch (Exception e)
                    {
                        LoggingService.Error(e, "Error Executing MapViewModel.LoadOperationCommand - Step Polylines");
                    }
                }

                foreach (var anchorId in Operation.Anchors)
                {
                    try
                    {
                        var portal = Operation.Portals.FirstOrDefault(p => p.Id.Equals(anchorId));
                        if (portal == null)
                            continue;

                        double.TryParse(portal.Lat, NumberStyles.Float, culture, out var portalLat);
                        double.TryParse(portal.Lng, NumberStyles.Float, culture, out var portalLng);

                        Pins.Add(new WasabeePin(new Pin()
                        {
                            Position = new Position(portalLat, portalLng),
                            Icon = BitmapDescriptorFactory.FromBundle($"marker_layer_{Operation.Color}")
                        })
                        {
                            Portal = portal
                        });
                    }
                    catch (Exception e)
                    {
                        LoggingService.Error(e, "Error Executing MapViewModel.LoadOperationCommand - Step Anchors");
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
                                Icon = BitmapDescriptorFactory.FromBundle($"{marker.Type}|{marker.State}"),
                                Label = GetMarkerNameFromTypeAndState(marker.Type, marker.State)
                            })
                        {
                            Portal = portal,
                            Marker = marker,
                            AssignedTo = marker.AssignedNickname
                        };

                        Pins.Add(pin);
                    }
                    catch (Exception e)
                    {
                        LoggingService.Error(e, "Error Executing MapViewModel.LoadOperationCommand - Step Markers");
                    }
                }
            }
            catch (NullReferenceException e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.LoadOperationCommand - Global");
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

        public IMvxAsyncCommand RefreshTeamsMembersPositionsCommand => new MvxAsyncCommand(RefreshTeamsMembersPositionsExecuted);
        private async Task RefreshTeamsMembersPositionsExecuted()
        {
            if (_loadingAgentsLocations)
                return;

            _loadingAgentsLocations = true;

            LoggingService.Trace("Executing MapViewModel.RefreshTeamsMembersPositionsCommand");

            try
            {
                var updatedTeams = new List<Models.Teams.TeamModel>();
                var userTeamsIds = (await _usersDatabase.GetUserTeams(_userSettingsService.GetLoggedUserGoogleId())).Select(x => x.Id);
                foreach (var teamId in userTeamsIds)
                {
                    var updatedData = await _wasabeeApiV1Service.GetTeam(teamId);
                    if (updatedData == null)
                        continue;

                    await _teamsDatabase.SaveTeamModel(updatedData);
                    updatedTeams.Add(updatedData);
                }

                var updatedAgents = new List<WasabeePlayerPin>();
                var agents = updatedTeams.SelectMany(t => t.Agents).Where(a => a.Lat != 0 && a.Lng != 0).DistinctBy(a => a.Name);
                foreach (var agent in agents)
                {
                    if (updatedAgents.Any(a => a.AgentName.Equals(agent.Name)))
                        continue;

                    var pin = new Pin()
                    {
                        Label = agent.Name,
                        Position = new Position(agent.Lat, agent.Lng),
                        Icon = BitmapDescriptorFactory.FromBundle("wasabee_player_marker")
                    };
                    var playerPin = new WasabeePlayerPin(pin) { AgentName = agent.Name };

                    if (!string.IsNullOrWhiteSpace(agent.Date) && DateTime.TryParse(agent.Date, out var agentDate))
                    {
                        var timeAgo = (DateTime.UtcNow - agentDate);
                        if (timeAgo.TotalMinutes > 1.0)
                        {
                            playerPin.TimeAgo = timeAgo.Minutes.ToString();
                            playerPin.Pin.Label += $" - {playerPin.TimeAgo}min ago";
                        }
                    }
                    updatedAgents.Add(playerPin);
                }

                foreach (var toRemove in from agent in updatedAgents
                                         where AgentsPins.Any(a => a.AgentName.Equals(agent.AgentName))
                                         select AgentsPins.First(a => a.AgentName.Equals(agent.AgentName)))
                {
                    AgentsPins.Remove(toRemove);
                }

                AgentsPins.AddRange(updatedAgents);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.RefreshTeamsMembersPositionsCommand");
            }
            finally
            {
                await RaisePropertyChanged(() => AgentsPins);
                _loadingAgentsLocations = false;
            }
        }


        public IMvxCommand RefreshOperationCommand => new MvxCommand(async () => await RefreshOperationExecuted());
        private async Task RefreshOperationExecuted()
        {
            if (IsBusy) return;
            LoggingService.Trace("Executing MapViewModel.RefreshOperationCommand");

            IsBusy = true;
            var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedOpId))
                return;

            var hasUpdated = false;
            try
            {
                var localData = await _operationsDatabase.GetOperationModel(selectedOpId);
                var updatedData = await _wasabeeApiV1Service.Operations_GetOperation(selectedOpId);

                if (!localData.Modified.Equals(updatedData.Modified))
                {
                    await _operationsDatabase.SaveOperationModel(updatedData);
                    hasUpdated = true;
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.RefreshOperationCommand");
            }
            finally
            {
                IsBusy = false;
                _userDialogs.Toast(hasUpdated ? "Operation data updated" : "You already have latest OP version");

                if (hasUpdated)
                {
                    await LoadOperationCommand.ExecuteAsync();
                    _messenger.Publish(new MessageFrom<MapViewModel>(this));
                }
            }

        }

        public IMvxCommand<MapThemeEnum> SwitchThemeCommand => new MvxCommand<MapThemeEnum>(SwitchThemeExecuted);
        private void SwitchThemeExecuted(MapThemeEnum mapTheme)
        {
            LoggingService.Trace("Executing MapViewModel.RefreshOperationCommand");

            MapTheme = mapTheme;
            switch (mapTheme)
            {
                case MapThemeEnum.Light:
                    _preferences.Set(UserSettingsKeys.MapTheme, nameof(MapThemeEnum.Light));
                    break;
                case MapThemeEnum.Dark:
                    _preferences.Set(UserSettingsKeys.MapTheme, nameof(MapThemeEnum.Dark));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapTheme), mapTheme, null);
            }
        }

        public IMvxAsyncCommand OpenInNavigationAppCommand => new MvxAsyncCommand(OpenInNavigationAppExecuted);
        private async Task OpenInNavigationAppExecuted()
        {
            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo("en-US");
                var uri = Device.RuntimePlatform switch
                {
                    Device.Android => "https://www.google.com/maps/search/?api=1&query=" +
                                      $"{SelectedWasabeePin.Pin.Position.Latitude.ToString(cultureInfo)}," +
                                      $"{SelectedWasabeePin.Pin.Position.Longitude.ToString(cultureInfo)}",
                    Device.iOS => "https://maps.apple.com/?ll=" +
                                  $"{SelectedWasabeePin.Pin.Position.Latitude.ToString(cultureInfo)}," +
                                  $"{SelectedWasabeePin.Pin.Position.Longitude.ToString(cultureInfo)}",
                    _ => throw new ArgumentOutOfRangeException(Device.RuntimePlatform)
                };

                if (await Launcher.CanOpenAsync(uri))
                    await Launcher.OpenAsync(uri);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.OpenInNavigationAppCommand");
            }
        }

        #endregion

        #region Private methods

        private string GetMarkerNameFromTypeAndState(string markerType, string markerState)
        {
            return markerType switch
            {
                "DestroyPortalAlert" => $"Destroy portal - {markerState}",
                "UseVirusPortalAlert" => $"Use virus - {markerState}",
                "CapturePortalMarker" => $"Capture portal - {markerState}",
                "FarmPortalMarker" => $"Farm keys - {markerState}",
                "LetDecayPortalAlert" => $"Let decay - {markerState}",
                "MeetAgentPortalMarker" => $"Meet Agent - {markerState}",
                "OtherPortalAlert" => $"Other - {markerState}",
                "RechargePortalAlert" => $"Recharge portal - {markerState}",
                "UpgradePortalAlert" => $"Upgrade portal - {markerState}",
                "CreateLinkAlert" => $"Create link - {markerState}",
                "ExcludeMarker" => "Exclude Marker",
                "GetKeyPortalMarker" => $"Get Key - {markerState}",
                "GotoPortalMarker" => $"Go to portal - {markerState}",
                _ => throw new ArgumentOutOfRangeException(markerType)
            };
        }

        #endregion
    }

    public enum MapThemeEnum
    {
        Light,
        Dark
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