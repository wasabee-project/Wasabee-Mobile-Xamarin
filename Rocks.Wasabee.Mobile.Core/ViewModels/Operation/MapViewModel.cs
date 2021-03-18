using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using MoreLinq;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.QueryModels;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms.GoogleMaps;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Operation
{
    public class MapViewModel : BaseViewModel
    {
        private static readonly Position DefaultPosition = new Position(45.767723, 4.835711); // Centers over Lyon, France

        private readonly OperationsDatabase _operationsDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly TeamAgentsDatabase _teamAgentsDatabase;
        private readonly UsersDatabase _usersDatabase;
        private readonly IPreferences _preferences;
        private readonly IMvxMessenger _messenger;
        private readonly IUserDialogs _userDialogs;
        private readonly IUserSettingsService _userSettingsService;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;

        private readonly MvxSubscriptionToken _token;
        private readonly MvxSubscriptionToken _tokenReload;
        private readonly MvxSubscriptionToken _tokenLiveLocation;
        private readonly MvxSubscriptionToken _tokenMarkerUpdated;
        private readonly MvxSubscriptionToken _tokenRefreshAllAgentsLocations;

        private bool _isLoadingAgentsLocations;

        public MapViewModel(OperationsDatabase operationsDatabase, TeamsDatabase teamsDatabase, TeamAgentsDatabase teamAgentsDatabase,
            UsersDatabase usersDatabase, IPreferences preferences, IMvxMessenger messenger,
            IUserDialogs userDialogs, IUserSettingsService userSettingsService, WasabeeApiV1Service wasabeeApiV1Service)
        {
            _operationsDatabase = operationsDatabase;
            _teamsDatabase = teamsDatabase;
            _teamAgentsDatabase = teamAgentsDatabase;
            _usersDatabase = usersDatabase;
            _preferences = preferences;
            _messenger = messenger;
            _userDialogs = userDialogs;
            _userSettingsService = userSettingsService;
            _wasabeeApiV1Service = wasabeeApiV1Service;

            _token = messenger.Subscribe<SelectedOpChangedMessage>(async msg =>
            {
                await LoadOperationCommand.ExecuteAsync();

                if (_preferences.Get(UserSettingsKeys.ShowAgentsFromAnyTeam, false) is false)
                {
                    // Force refresh agents pins to only show current OP agents
                    AgentsPins.Clear();
                    await RaisePropertyChanged(() => AgentsPins);
                }

                await RefreshTeamsMembersPositionsCommand.ExecuteAsync(string.Empty);
            });
            _tokenReload = messenger.Subscribe<MessageFrom<OperationRootTabbedViewModel>>(async msg => await LoadOperationCommand.ExecuteAsync());
            _tokenLiveLocation = messenger.Subscribe<TeamAgentLocationUpdatedMessage>(async msg => await RefreshTeamAgentPositionCommand.ExecuteAsync(msg));
            _tokenMarkerUpdated = messenger.Subscribe<MarkerDataChangedMessage>(msg => UpdateMarker(msg));
            _tokenRefreshAllAgentsLocations = messenger.Subscribe<RefreshAllAgentsLocationsMessage>(async msg => await RefreshTeamsMembersPositionsCommand.ExecuteAsync(string.Empty));
        }

        public override async Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);
            LoggingService.Trace("Navigated to MapViewModel");

            MapTheme = _preferences.Get(UserSettingsKeys.MapTheme, nameof(MapThemeEnum.GoogleLight)) switch
            {
                nameof(MapThemeEnum.Enlightened) => MapThemeEnum.Enlightened,
                nameof(MapThemeEnum.IntelDefault) => MapThemeEnum.IntelDefault,
                _ => MapThemeEnum.GoogleLight
            };
            MapType = _preferences.Get(UserSettingsKeys.MapType, nameof(MapType.Street)) switch
            {
                nameof(MapType.Street) => MapType.Street,
                nameof(MapType.Hybrid) => MapType.Hybrid,
                _ => MapType.Street
            };

            await base.Initialize();

            IsLocationAvailable = await CheckAndAskForLocationPermissions();

            await LoadOperationCommand.ExecuteAsync();
            await RefreshTeamsMembersPositionsCommand.ExecuteAsync(string.Empty);
        }
        
        #region Properties

        public bool IsLoading { get; set; }

        public Pin? SelectedPin
        {
            get => SelectedWasabeePin?.Pin;
            set
            {
                SelectedWasabeePin = value != null
                    ? Anchors.FirstOrDefault(x => x.Pin == value) ?? Markers.FirstOrDefault(x => x.Pin == value)
                    : null;
                RaisePropertyChanged(() => SelectedPin);
            }
        }

        public OperationModel? Operation { get; set; }
        public WasabeePin? SelectedWasabeePin { get; set; }

        public MvxObservableCollection<Polyline> Links { get; set; } = new MvxObservableCollection<Polyline>();
        public MvxObservableCollection<WasabeePin> Anchors { get; set; } = new MvxObservableCollection<WasabeePin>();
        public MvxObservableCollection<WasabeePin> Markers { get; set; } = new MvxObservableCollection<WasabeePin>();
        public MvxObservableCollection<WasabeeAgentPin> AgentsPins { get; set; } = new MvxObservableCollection<WasabeeAgentPin>();

        public MapSpan OperationMapRegion { get; set; } = MapSpan.FromCenterAndRadius(DefaultPosition, Distance.FromKilometers(5));

        public MapSpan? VisibleRegion { get; set; }

        public bool IsLocationAvailable { get; set; } = false;

        public MapThemeEnum MapTheme { get; set; } = MapThemeEnum.GoogleLight;
        public MapType MapType { get; set; } = MapType.Street;
        public bool IsStylingAvailable { get; set; } = true;

        public bool IsLayerChooserVisible { get; set; } = false;
        public bool IsLayerLinksActivated { get; set; } = true;
        public bool IsLayerMarkersActivated { get; set; } = true;
        public bool IsLayerAnchorsActivated { get; set; } = true;
        public bool IsLayerAgentsActivated { get; set; } = true;

        #endregion

        #region Commands

        public IMvxCommand CloseDetailPanelCommand => new MvxCommand(() => SelectedPin = null);

        public IMvxCommand MoveToPortalCommand => new MvxCommand(MoveToPortalExecuted);
        private void MoveToPortalExecuted()
        {
            LoggingService.Trace("Executing MapViewModel.MoveToPortalCommand");

            if (SelectedWasabeePin == null)
                return;

            var region = MapSpan.FromCenterAndRadius(SelectedWasabeePin.Pin.Position, Distance.FromMeters(100));
            if (VisibleRegion == region)
                RaisePropertyChanged(() => VisibleRegion);
            else
                VisibleRegion = region;
        }

        public IMvxAsyncCommand LoadOperationCommand => new MvxAsyncCommand(LoadOperationExecuted);
        private async Task LoadOperationExecuted()
        {
            if (IsLoading) return;
            IsLoading = true;

            LoggingService.Trace("Executing MapViewModel.LoadOperationCommand");

            var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedOpId))
                return;

            Links.Clear();
            Anchors.Clear();
            Markers.Clear();

            Operation = await _operationsDatabase.GetOperationModel(selectedOpId);
            if (Operation == null)
            {
                IsLoading = false;
                return;
            }

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

                        var baseLinkColor = WasabeeColorsHelper.GetColorFromWasabeeName(link.Color, Operation.Color);
                        if (link.Completed)
                            baseLinkColor = baseLinkColor.MultiplyAlpha(0.5);

                        Links.Add(
                            new Polyline()
                            {
                                StrokeColor = baseLinkColor,
                                StrokeWidth = link.Completed ? 1 : 2,
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

                        Anchors.Add(new WasabeePin(new Pin()
                        {
                            Position = new Position(portalLat, portalLng),
                            Icon = BitmapDescriptorFactory.FromBundle($"pin_{WasabeeColorsHelper.GetPinColorNameFromHex(Operation.Color)}")
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
                            AssignedTo = marker.AssignedNickname // TODO Replace this
                        };

                        Markers.Add(pin);
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
                UpdateMapRegion();

                // Message data set to true to force move mapview
                _messenger.Publish(new MessageFrom<MapViewModel>(this, true));
                IsLoading = false;
            }
        }

        public IMvxAsyncCommand<string> RefreshTeamsMembersPositionsCommand => new MvxAsyncCommand<string>(RefreshTeamsMembersPositionsExecuted);
        private async Task RefreshTeamsMembersPositionsExecuted(string teamId)
        {
            if (Operation is null)
                return;

            if (_isLoadingAgentsLocations)
                return;

            _isLoadingAgentsLocations = true;

            LoggingService.Trace("Executing MapViewModel.RefreshTeamsMembersPositionsCommand");

            try
            {
                var showFromAnyTeams = _preferences.Get(UserSettingsKeys.ShowAgentsFromAnyTeam, false);

                // Get all teams by default
                var userTeamsIds = showFromAnyTeams ?
                    (await _usersDatabase.GetUserTeams(_userSettingsService.GetLoggedUserGoogleId())).Select(x => x.Id).ToList() :
                    Operation.TeamList.Select(x => x.TeamId).ToList();

                // If teamId is specified, refresh only this one
                if (!string.IsNullOrWhiteSpace(teamId))
                {
                    // but only if Operation is assigned to the team or showFromAnyTeams is true from Settings
                    if (Operation.TeamList.Any(x => x.TeamId.Equals(teamId)) || showFromAnyTeams)
                    {
                        userTeamsIds.Clear();
                        userTeamsIds.Add(teamId);
                    }
                    else
                    {
                        userTeamsIds.Clear();
                        _isLoadingAgentsLocations = false;

                        LoggingService.Trace($"Nothing to refresh, teamId set to '{teamId}', Setting showFromAnyTeams is set to {showFromAnyTeams}");

                        return;
                    }
                }

                if (!userTeamsIds.Any())
                {
                    LoggingService.Trace("Nothing to refresh, teams contains no elements");
                    return;
                }

                var updatedTeams = await _wasabeeApiV1Service.Teams_GetTeams(new GetTeamsQuery(userTeamsIds));
                if (updatedTeams.Any())
                    await _teamsDatabase.SaveTeamsModels(updatedTeams);

                var agents = updatedTeams
                    .SelectMany(x => x.Agents)
                    .Where(a => a.State && a.Lat != 0 && a.Lng != 0)
                    .DistinctBy(a => a.Id)
                    .ToList();

                if (agents.IsNullOrEmpty())
                {
                    LoggingService.Trace("Nothing to refresh, teams agents contains no elements");
                    return;
                }

                var wasabeeAgentPins = new List<WasabeeAgentPin>();
                var loggedUserId = _userSettingsService.GetLoggedUserGoogleId();
                var opTeamsIds = Operation.TeamList.Select(x => x.TeamId);
                foreach (var agent in agents)
                {
                    if (wasabeeAgentPins.Any(a => a.AgentId.Equals(agent.Id)))
                        continue;

                    var agentTeams = updatedTeams.Where(t => t.Agents.Any(a => a.Id.Equals(agent.Id)));
                    var isAgentAssignedToOperation = agentTeams.Any(x => opTeamsIds.Any(tId => tId.Equals(x.Id)));
                    var isCurrentUser = agent.Id.Equals(loggedUserId);

                    var updatedAgentPin = CreateAgentPin(agent, isAgentAssignedToOperation, isCurrentUser);
                    wasabeeAgentPins.Add(updatedAgentPin);
                }

                foreach (var toRemove in from agentPin in wasabeeAgentPins
                                         where AgentsPins.Any(a => a.AgentId.Equals(agentPin.AgentId))
                                         select AgentsPins.First(a => a.AgentId.Equals(agentPin.AgentId)))
                {
                    AgentsPins.Remove(toRemove);
                }

                AgentsPins.AddRange(wasabeeAgentPins);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.RefreshTeamsMembersPositionsCommand");
            }
            finally
            {
                await RaisePropertyChanged(() => AgentsPins);
                _isLoadingAgentsLocations = false;
            }
        }



        public IMvxAsyncCommand<TeamAgentLocationUpdatedMessage> RefreshTeamAgentPositionCommand => new MvxAsyncCommand<TeamAgentLocationUpdatedMessage>(RefreshTeamAgentPositionExecuted);
        private async Task RefreshTeamAgentPositionExecuted(TeamAgentLocationUpdatedMessage message)
        {
            if (Operation == null)
                return;

            if (_isLoadingAgentsLocations)
                return;

            _isLoadingAgentsLocations = true;

            LoggingService.Trace("Executing MapViewModel.RefreshTeamAgentPositionCommand");

            try
            {
                // Refresh the whole team
                if (string.IsNullOrEmpty(message.UserId))
                {
                    _isLoadingAgentsLocations = false;
                    await RefreshTeamsMembersPositionsExecuted(message.TeamId);

                    return;
                }

                // Refresh only agent
                var agentId = message.UserId;
                var agentTeams = await _teamsDatabase.GetTeamsForAgent(agentId);
                if (!agentTeams.Any())
                    return;

                var showFromAnyTeams = _preferences.Get(UserSettingsKeys.ShowAgentsFromAnyTeam, false);
                var opTeamsIds = Operation.TeamList.Select(x => x.TeamId);

                var isAgentAssignedToOperation = agentTeams.Any(x => opTeamsIds.Any(tId => tId.Equals(x.Id)));

                // Do not refresh agent if he's in a team not assigned to Operation and setting ShowAgentsFromAnyTeam is false
                // but refresh if it's current user
                var loggedUserId = _userSettingsService.GetLoggedUserGoogleId();
                var isCurrentUser = agentId.Equals(loggedUserId);
                if (isAgentAssignedToOperation is false && showFromAnyTeams is false)
                {
                    if (isCurrentUser is false)
                        return;
                }

                var agent = await _teamAgentsDatabase.GetTeamAgent(agentId);
                if (agent == null)
                {
                    var team = await _teamsDatabase.GetTeam(message.TeamId);
                    if (team == null || team.Agents.All(x => x.Id != agentId))
                    {
                        team = await _wasabeeApiV1Service.Teams_GetTeam(message.TeamId);
                        if (team == null)
                            return;

                        await _teamsDatabase.SaveTeamModel(team);
                    }

                    agent = team.Agents.FirstOrDefault(x => x.Id.Equals(agentId));
                    if (agent == null)
                        return;
                }

                if (DateTime.Now - agent.LastUpdatedAt < TimeSpan.FromSeconds(5))
                    return;

                var updatedAgent = await _wasabeeApiV1Service.Agents_GetAgent(agentId);
                if (updatedAgent == null || (updatedAgent.Lat == 0 && updatedAgent.Lng == 0))
                    return;

                var updatedAgentPin = CreateAgentPin(updatedAgent, isAgentAssignedToOperation, isCurrentUser);
                var toRemove = AgentsPins.FirstOrDefault(a => a.AgentId.Equals(updatedAgentPin.AgentId));
                if (toRemove != null)
                    AgentsPins.Remove(toRemove);

                AgentsPins.Add(updatedAgentPin);

                await _teamAgentsDatabase.SaveTeamAgentModel(updatedAgent);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.RefreshTeamAgentPositionCommand");
            }
            finally
            {
                await RaisePropertyChanged(() => AgentsPins);
                _isLoadingAgentsLocations = false;
            }
        }

        public IMvxCommand<MapThemeEnum> SwitchThemeCommand => new MvxCommand<MapThemeEnum>(SwitchThemeExecuted);
        private void SwitchThemeExecuted(MapThemeEnum mapTheme)
        {
            LoggingService.Trace("Executing MapViewModel.SwitchThemeCommand");

            MapTheme = mapTheme;
            switch (mapTheme)
            {
                case MapThemeEnum.GoogleLight:
                    _preferences.Set(UserSettingsKeys.MapTheme, nameof(MapThemeEnum.GoogleLight));
                    break;
                case MapThemeEnum.Enlightened:
                    _preferences.Set(UserSettingsKeys.MapTheme, nameof(MapThemeEnum.Enlightened));
                    break;
                case MapThemeEnum.IntelDefault:
                    _preferences.Set(UserSettingsKeys.MapTheme, nameof(MapThemeEnum.IntelDefault));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapTheme), mapTheme, null);
            }
        }

        public IMvxCommand<MapType> SwitchMapTypeCommand => new MvxCommand<MapType>(SwitchMapTypeExecuted);
        private void SwitchMapTypeExecuted(MapType mapType)
        {
            LoggingService.Trace("Executing MapViewModel.SwitchMapTypeCommand");

            MapType = mapType;
            switch (mapType)
            {
                case MapType.Street:
                    _preferences.Set(UserSettingsKeys.MapType, nameof(MapType.Street));
                    IsStylingAvailable = true;
                    break;
                case MapType.Hybrid:
                    _preferences.Set(UserSettingsKeys.MapType, nameof(MapType.Hybrid));
                    IsStylingAvailable = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null);
            }
        }

        public IMvxAsyncCommand OpenInNavigationAppCommand => new MvxAsyncCommand(OpenInNavigationAppExecuted);
        private async Task OpenInNavigationAppExecuted()
        {
            LoggingService.Trace("Executing MapViewModel.OpenInNavigationAppCommand");
            if (SelectedWasabeePin == null) return;

            try
            {
                var location = new Location(SelectedWasabeePin.Pin.Position.Latitude, SelectedWasabeePin.Pin.Position.Longitude);
                await Xamarin.Essentials.Map.OpenAsync(location);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.OpenInNavigationAppCommand");
            }
        }

        #endregion

        #region Private methods
        
        private async Task<bool> CheckAndAskForLocationPermissions()
        {
            LoggingService.Trace("MapViewModel - Checking location permissions");
            
            var statusLocationAlways = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            LoggingService.Trace($"Permissions Status : LocationWhenInUse={statusLocationAlways}");

            if (statusLocationAlways == PermissionStatus.Granted)
                return true;
            
            var requestPermission = true;
            var showRationale = Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>();
            if (showRationale)
                requestPermission = await _userDialogs.ConfirmAsync(
                    "To show your position on the map, please set the permission to 'When in use'.",
                    "Permissions required",
                    "Ok", "Cancel");

            if (!requestPermission)
                return false;

            LoggingService.Trace("Requesting location permissions");
            statusLocationAlways = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            LoggingService.Trace($"Permissions Status : LocationWhenInUse={statusLocationAlways}");

            if (statusLocationAlways != PermissionStatus.Granted)
            {
                LoggingService.Trace("User didn't granted geolocation permissions");

                _userDialogs.Alert("Geolocation permission is required !");
                return false;
            }

            LoggingService.Info("User has granted geolocation permissions");
            return true;
        }

        private WasabeeAgentPin CreateAgentPin(Models.Teams.TeamAgentModel agent, bool isAgentAssignedToOperation, bool isCurrentUser = false)
        {
            var pin = new Pin()
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
                playerPin.Pin.Label += $" - {playerPin.TimeAgo} ago";
            }

            return playerPin;
        }

        private void UpdateMapRegion()
        {
            var lowestLat = 0.0d;
            var highestLat = 0.0d;
            var lowestLong = 0.0d;
            var highestLong = 0.0d;
            var finalLat = 0.0d;
            var finalLong = 0.0d;
            var distance = 0.0d;

            if (Markers.Any() && Anchors.Any())
            {
                lowestLat = Math.Min(Anchors.Min(p => p.Pin.Position.Latitude), Markers.Min(p => p.Pin.Position.Latitude));
                highestLat = Math.Max(Anchors.Max(p => p.Pin.Position.Latitude), Markers.Max(p => p.Pin.Position.Latitude));
                lowestLong = Math.Min(Anchors.Min(p => p.Pin.Position.Longitude), Markers.Min(p => p.Pin.Position.Longitude));
                highestLong = Math.Max(Anchors.Max(p => p.Pin.Position.Longitude), Markers.Max(p => p.Pin.Position.Longitude));
                finalLat = (lowestLat + highestLat) / 2;
                finalLong = (lowestLong + highestLong) / 2;
                distance = DistanceCalculation.GeoCodeCalc.CalcDistance(lowestLat, lowestLong, highestLat,
                    highestLong, DistanceCalculation.GeoCodeCalcMeasurement.Kilometers);
            }
            else if (Markers.Any() && !Anchors.Any())
            {
                lowestLat = Markers.Min(p => p.Pin.Position.Latitude);
                highestLat = Markers.Max(p => p.Pin.Position.Latitude);
                lowestLong = Markers.Min(p => p.Pin.Position.Longitude);
                highestLong = Markers.Max(p => p.Pin.Position.Longitude);
                finalLat = (lowestLat + highestLat) / 2;
                finalLong = (lowestLong + highestLong) / 2;
                distance = DistanceCalculation.GeoCodeCalc.CalcDistance(lowestLat, lowestLong, highestLat,
                    highestLong, DistanceCalculation.GeoCodeCalcMeasurement.Kilometers);
            }
            else if (!Markers.Any() && Anchors.Any())
            {
                lowestLat = Anchors.Min(p => p.Pin.Position.Latitude);
                highestLat = Anchors.Max(p => p.Pin.Position.Latitude);
                lowestLong = Anchors.Min(p => p.Pin.Position.Longitude);
                highestLong = Anchors.Max(p => p.Pin.Position.Longitude);
                finalLat = (lowestLat + highestLat) / 2;
                finalLong = (lowestLong + highestLong) / 2;
                distance = DistanceCalculation.GeoCodeCalc.CalcDistance(lowestLat, lowestLong, highestLat,
                    highestLong, DistanceCalculation.GeoCodeCalcMeasurement.Kilometers);

            }

            if (distance != 0.0d)
                OperationMapRegion = MapSpan.FromCenterAndRadius(new Position(finalLat, finalLong), Distance.FromKilometers(distance));
            else
                OperationMapRegion = MapSpan.FromCenterAndRadius(DefaultPosition, Distance.FromKilometers(5000));
        }

        private void UpdateMarker(MarkerDataChangedMessage updateMessage)
        {
            if (Operation == null)
                return;

            if (!updateMessage.OperationId.Equals(Operation.Id))
                return;

            var marker = Markers.FirstOrDefault(x => x.Marker.Id.Equals(updateMessage.MarkerModel.Id));
            if (marker != null)
            {
                var culture = CultureInfo.GetCultureInfo("en-US");
                var portal = Operation.Portals.First(x => x.Id.Equals(updateMessage.MarkerModel.PortalId));
                double.TryParse(portal.Lat, NumberStyles.Float, culture, out var portalLat);
                double.TryParse(portal.Lng, NumberStyles.Float, culture, out var portalLng);

                var pin = new WasabeePin(
                    new Pin()
                    {
                        Position = new Position(portalLat, portalLng),
                        Icon = BitmapDescriptorFactory.FromBundle($"{updateMessage.MarkerModel.Type}|{updateMessage.MarkerModel.State}"),
                        Label = GetMarkerNameFromTypeAndState(updateMessage.MarkerModel.Type, updateMessage.MarkerModel.State)
                    })
                {
                    Portal = portal,
                    Marker = updateMessage.MarkerModel,
                    AssignedTo = updateMessage.MarkerModel.AssignedNickname
                };

                Markers.Remove(marker);
                Markers.Add(pin);

                _messenger.Publish(new MessageFrom<MapViewModel>(this));

                // If assigned to current user
                if (updateMessage.MarkerModel.AssignedTo.Equals(_userSettingsService.GetLoggedUserGoogleId()))
                    _messenger.Publish(new MessageFor<AssignmentsListViewModel>(this));
            }
        }

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
        GoogleLight,
        Enlightened,
        IntelDefault
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