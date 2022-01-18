using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using MoreLinq;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Helpers.Xaml;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Agent;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.QueryModels;
using Rocks.Wasabee.Mobile.Core.Resources.I18n;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation.MapElements;
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
    public class MapViewModel : BasePageInTabbedPageViewModel
    {
        private static readonly Position DefaultPosition = new Position(45.767723, 4.835711); // Centers over Lyon, France
        private static readonly CultureInfo ConversionCulture = CultureInfo.GetCultureInfo("en-US");

        private readonly OperationsDatabase _operationsDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly AgentsDatabase _agentsDatabase;
        private readonly UsersDatabase _usersDatabase;
        private readonly IPreferences _preferences;
        private readonly IMvxMessenger _messenger;
        private readonly IClipboard _clipboard;
        private readonly IMap _map;
        private readonly IUserDialogs _userDialogs;
        private readonly IUserSettingsService _userSettingsService;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly IDialogNavigationService _dialogNavigationService;

        private MvxSubscriptionToken? _token;
        private MvxSubscriptionToken? _tokenReload;
        private MvxSubscriptionToken? _tokenLiveLocation;
        private MvxSubscriptionToken? _tokenLinkUpdated;
        private MvxSubscriptionToken? _tokenMarkerUpdated;
        private MvxSubscriptionToken? _tokenRefreshAllAgentsLocations;

        private bool _isLoadingAgentsLocations;

        public MapViewModel(OperationsDatabase operationsDatabase, TeamsDatabase teamsDatabase, AgentsDatabase agentsDatabase,
            UsersDatabase usersDatabase, IPreferences preferences, IMvxMessenger messenger, IClipboard clipboard, IMap map,
            IUserDialogs userDialogs, IUserSettingsService userSettingsService, WasabeeApiV1Service wasabeeApiV1Service, IDialogNavigationService dialogNavigationService)
        {
            _operationsDatabase = operationsDatabase;
            _teamsDatabase = teamsDatabase;
            _agentsDatabase = agentsDatabase;
            _usersDatabase = usersDatabase;
            _preferences = preferences;
            _messenger = messenger;
            _clipboard = clipboard;
            _map = map;
            _userDialogs = userDialogs;
            _userSettingsService = userSettingsService;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _dialogNavigationService = dialogNavigationService;
        }

        public override async Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);
            LoggingService.Trace("Navigated to MapViewModel");

            MapTheme = _preferences.Get(UserSettingsKeys.MapTheme, nameof(MapThemeEnum.GoogleLight)) switch
            {
                nameof(MapThemeEnum.Enlightened) => MapThemeEnum.Enlightened,
                nameof(MapThemeEnum.IntelDefault) => MapThemeEnum.IntelDefault,
                nameof(MapThemeEnum.RedIntel) => MapThemeEnum.RedIntel,
                _ => MapThemeEnum.GoogleLight
            };

            MapType = _preferences.Get(UserSettingsKeys.MapType, nameof(MapType.Street)) switch
            {
                nameof(MapType.Street) => MapType.Street,
                nameof(MapType.Hybrid) => MapType.Hybrid,
                _ => MapType.Street
            };

            var rawMapLayersConfig = _preferences.Get(UserSettingsKeys.MapLayers, string.Empty);
            var config = string.IsNullOrEmpty(rawMapLayersConfig) ? null : JsonConvert.DeserializeObject<MapLayersConfig>(rawMapLayersConfig);
            if (config != null)
            {
                IsLayerLinksActivated = config.Links;
                IsLayerMarkersActivated = config.Markers;
                IsLayerAnchorsActivated = config.Anchors;
                IsLayerAgentsActivated = config.Agents;
                IsLayerZonesActivated = config.Zones;
            }

            IsStylingAvailable = MapType == MapType.Street;

            await base.Initialize();

            IsLocationAvailable = await CheckAndAskForLocationPermissions();
        }

        public override async void ViewAppearing()
        {
            base.ViewAppearing();

            _token = _messenger.Subscribe<SelectedOpChangedMessage>(async msg =>
            {
                await LoadOperationCommand.ExecuteAsync(true);

                if (_preferences.Get(UserSettingsKeys.ShowAgentsFromAnyTeam, false) is false)
                {
                    // Force refresh agents pins to only show current OP agents
                    Agents.Clear();
                    await RaisePropertyChanged(() => Agents);
                }

                await RefreshTeamsMembersPositionsCommand.ExecuteAsync(string.Empty);
            });
            _tokenReload ??= _messenger.Subscribe<MessageFrom<OperationRootTabbedViewModel>>(async msg => await LoadOperationCommand.ExecuteAsync(false));
            _tokenLiveLocation ??= _messenger.Subscribe<TeamAgentLocationUpdatedMessage>(async msg => await RefreshTeamAgentPositionCommand.ExecuteAsync(msg));
            _tokenLinkUpdated ??= _messenger.Subscribe<LinkDataChangedMessage>(UpdateLink);
            _tokenMarkerUpdated ??= _messenger.Subscribe<MarkerDataChangedMessage>(UpdateMarker);
            _tokenRefreshAllAgentsLocations ??= _messenger.Subscribe<RefreshAllAgentsLocationsMessage>(async msg => await RefreshTeamsMembersPositionsCommand.ExecuteAsync(string.Empty));

            await LoadOperationCommand.ExecuteAsync(false);
            await RefreshTeamsMembersPositionsCommand.ExecuteAsync(string.Empty);
        }

        public override void Destroy()
        {
            _token?.Dispose();
            _token = null;
            _tokenReload?.Dispose();
            _tokenReload = null;
            _tokenLiveLocation?.Dispose();
            _tokenLiveLocation = null;
            _tokenLinkUpdated?.Dispose();
            _tokenLinkUpdated = null;
            _tokenMarkerUpdated?.Dispose();
            _tokenMarkerUpdated = null;
            _tokenRefreshAllAgentsLocations?.Dispose();
            _tokenRefreshAllAgentsLocations = null;
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

                IsMarkerDetailAvailable = !string.IsNullOrWhiteSpace(SelectedWasabeePin?.Marker.Id);
            }
        }

        public OperationModel? Operation { get; set; }
        public WasabeePin? SelectedWasabeePin { get; set; }
        public WasabeeAgentPin? SelectedAgentPin { get; set; }

        public MvxObservableCollection<WasabeeLink> Links { get; set; } = new MvxObservableCollection<WasabeeLink>();
        public MvxObservableCollection<WasabeePin> Anchors { get; set; } = new MvxObservableCollection<WasabeePin>();
        public MvxObservableCollection<WasabeePin> Markers { get; set; } = new MvxObservableCollection<WasabeePin>();
        public MvxObservableCollection<WasabeeAgentPin> Agents { get; set; } = new MvxObservableCollection<WasabeeAgentPin>();
        public MvxObservableCollection<WasabeeOperationZone> Zones { get; set; } = new MvxObservableCollection<WasabeeOperationZone>();

        public MapSpan OperationMapRegion { get; set; } = MapSpan.FromCenterAndRadius(DefaultPosition, Distance.FromKilometers(5));

        public MapSpan? VisibleRegion { get; set; }

        public bool IsLocationAvailable { get; set; } = false;

        public MapThemeEnum MapTheme { get; set; } = MapThemeEnum.GoogleLight;
        public MapType MapType { get; set; } = MapType.Street;
        public bool IsStylingAvailable { get; set; } = true;

        public bool IsLayerChooserVisible { get; set; } = false;

        public bool IsMarkerDetailAvailable { get; set; }

        public bool IsAgentListVisible { get; set; }

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

        public IMvxCommand MoveToAgentCommand => new MvxCommand(MoveToAgentExecuted);
        private void MoveToAgentExecuted()
        {
            LoggingService.Trace("Executing MapViewModel.MoveToAgentCommand");

            if (SelectedAgentPin == null)
                return;

            var region = MapSpan.FromCenterAndRadius(SelectedAgentPin.Pin.Position, Distance.FromMeters(100));
            if (VisibleRegion == region)
                RaisePropertyChanged(() => VisibleRegion);
            else
                VisibleRegion = region;
        }

        public IMvxAsyncCommand<bool> LoadOperationCommand => new MvxAsyncCommand<bool>(LoadOperationExecuted);
        private async Task LoadOperationExecuted(bool forceResetMapView)
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

                        double.TryParse(portal.Lat, NumberStyles.Float, ConversionCulture, out var portalLat);
                        double.TryParse(portal.Lng, NumberStyles.Float, ConversionCulture, out var portalLng);

                        Anchors.Add(new WasabeePin(new Pin()
                        {
                            Position = new Position(portalLat, portalLng),
                            Icon = BitmapDescriptorFactory.FromBundle($"pin_{Operation.Color}")
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
                            new Pin()
                            {
                                Position = new Position(portalLat, portalLng),
                                Icon = BitmapDescriptorFactory.FromBundle($"{marker.Type}|{marker.State}"),
                                Label = GetMarkerNameFromTypeAndState(marker.Type, marker.State)
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
                        LoggingService.Error(e, "Error Executing MapViewModel.LoadOperationCommand - Step Markers");
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
                        LoggingService.Error(e, "Error Executing MapViewModel.LoadOperationCommand - Step Zones");
                    }
                }
            }
            catch (NullReferenceException e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.LoadOperationCommand - Global");
            }
            finally
            {
                await RaisePropertyChanged(() => Links);
                await RaisePropertyChanged(() => Anchors);
                await RaisePropertyChanged(() => Markers);
                await RaisePropertyChanged(() => Zones);

                UpdateMapRegion();

                _messenger.Publish(new MessageFrom<MapViewModel>(this, forceResetMapView));
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
                                         where Agents.Any(a => a.AgentId.Equals(agentPin.AgentId))
                                         select Agents.First(a => a.AgentId.Equals(agentPin.AgentId)))
                {
                    Agents.Remove(toRemove);
                }

                Agents.AddRange(wasabeeAgentPins);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.RefreshTeamsMembersPositionsCommand");
            }
            finally
            {
                await RaisePropertyChanged(() => Agents);
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

                var agent = await _agentsDatabase.GetAgent(agentId);
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
                var toRemove = Agents.FirstOrDefault(a => a.AgentId.Equals(updatedAgentPin.AgentId));
                if (toRemove != null)
                    Agents.Remove(toRemove);

                Agents.Add(updatedAgentPin);

                await _agentsDatabase.SaveTeamAgentModel(updatedAgent);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.RefreshTeamAgentPositionCommand");
            }
            finally
            {
                await RaisePropertyChanged(() => Agents);
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
                case MapThemeEnum.RedIntel:
                    _preferences.Set(UserSettingsKeys.MapTheme, nameof(MapThemeEnum.RedIntel));
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
                var coordinates = $"{SelectedWasabeePin.Pin.Position.Latitude},{SelectedWasabeePin.Pin.Position.Longitude}";
                var location = new Location(SelectedWasabeePin.Pin.Position.Latitude, SelectedWasabeePin.Pin.Position.Longitude);

                if (string.IsNullOrEmpty(coordinates) is false)
                {
                    await _clipboard.SetTextAsync(coordinates);
                    if (_clipboard.HasText)
                        _userDialogs.Toast(TranslateExtension.GetValue("Toast_CoordinatesCopied"));
                }

                await _map.OpenAsync(location);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.OpenInNavigationAppCommand");
            }
        }

        public IMvxAsyncCommand<WasabeePin> ShowMarkerDetailCommand => new MvxAsyncCommand<WasabeePin>(ShowMarkerDetailExecuted);
        private async Task ShowMarkerDetailExecuted(WasabeePin pin)
        {
            if (IsMarkerDetailAvailable is false || Operation is null)
                return;

            var assignmentData = CreateMarkerAssignmentData(pin.Marker);
            assignmentData.ShowAssignee = true;
            await _dialogNavigationService.Navigate<MarkerAssignmentDialogViewModel, MarkerAssignmentData>(assignmentData);
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
            LoggingService.Trace("MapViewModel - Checking location permissions");

            var statusLocationAlways = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            LoggingService.Trace($"Permissions Status : LocationWhenInUse={statusLocationAlways}");

            if (statusLocationAlways == PermissionStatus.Granted)
                return true;

            var requestPermission = true;
            var showRationale = Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>();
            if (showRationale)
                requestPermission = await _userDialogs.ConfirmAsync(
                    Strings.Dialogs_Warning_PermissionWhenInUse,
                    Strings.Dialog_Warning_PermissionsRequired,
                    Strings.Global_Ok,
                    Strings.Global_Cancel);

            if (!requestPermission)
                return false;

            LoggingService.Trace("Requesting location permissions");
            statusLocationAlways = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            LoggingService.Trace($"Permissions Status : LocationWhenInUse={statusLocationAlways}");

            if (statusLocationAlways != PermissionStatus.Granted)
            {
                LoggingService.Trace("User didn't granted geolocation permissions");

                _userDialogs.Alert(Strings.Dialogs_Warning_LocationPermissionRequired);
                return false;
            }

            LoggingService.Info("User has granted geolocation permissions");
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
                baseLinkColor = baseLinkColor.MultiplyAlpha(0.5);

            var wasabeeLink = new WasabeeLink(
                new Polyline()
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
                if (string.IsNullOrEmpty(playerPin.TimeAgo) is false)
                    playerPin.Pin.Label += $" - {string.Format(Strings.Map_Text_TimeAgo, playerPin.TimeAgo)}";
            }

            return playerPin;
        }

        private bool BuildZonePolygon(Polygon polygonToBuild, IEnumerable<ZonePointModel> points)
        {
            try
            {
                foreach (var point in points)
                {
                    double.TryParse(point.Lat, NumberStyles.Float, ConversionCulture, out var pointLat);
                    double.TryParse(point.Lng, NumberStyles.Float, ConversionCulture, out var pointLng);

                    polygonToBuild.Positions.Add(new Position(pointLat, pointLng));
                }

                return true;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing MapViewModel.BuildZonePolygon");
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

        private void UpdateLink(LinkDataChangedMessage updateMessage)
        {
            if (Operation == null)
                return;

            if (!updateMessage.OperationId.Equals(Operation.Id))
                return;

            var oldLink = Links.FirstOrDefault(x => x.LinkId.Equals(updateMessage.LinkData.Id));
            if (oldLink != null)
            {
                var newLink = CreateWasabeeLink(updateMessage.LinkData);
                if (newLink is null)
                    return;

                Links.Remove(oldLink);
                Links.Add(newLink);

                _messenger.Publish(new MessageFrom<MapViewModel>(this, false));

                // If assigned to current user
                if (updateMessage.LinkData.Assignments.IsNotNullOrEmpty() &&
                    updateMessage.LinkData.Assignments.Contains(_userSettingsService.GetLoggedUserGoogleId()))
                    _messenger.Publish(new MessageFor<AssignmentsListViewModel>(this));
            }
        }

        private void UpdateMarker(MarkerDataChangedMessage updateMessage)
        {
            if (Operation == null)
                return;

            if (!updateMessage.OperationId.Equals(Operation.Id))
                return;

            var marker = Markers.FirstOrDefault(x => x.Marker.Id.Equals(updateMessage.MarkerData.Id));
            if (marker != null)
            {
                Markers.Remove(marker);

                var hideCompletedMarkersSetting = _preferences.Get(UserSettingsKeys.HideCompletedMarkers, false);
                if (hideCompletedMarkersSetting && updateMessage.MarkerData.State is TaskState.Completed)
                {
                    CloseDetailPanelCommand.Execute();
                }
                else
                {
                    var culture = CultureInfo.GetCultureInfo("en-US");
                    var portal = Operation.Portals.First(x => x.Id.Equals(updateMessage.MarkerData.PortalId));
                    double.TryParse(portal.Lat, NumberStyles.Float, culture, out var portalLat);
                    double.TryParse(portal.Lng, NumberStyles.Float, culture, out var portalLng);

                    var pin = new WasabeePin(
                        new Pin()
                        {
                            Position = new Position(portalLat, portalLng),
                            Icon = BitmapDescriptorFactory.FromBundle(
                                $"{updateMessage.MarkerData.Type}|{updateMessage.MarkerData.State}"),
                            Label = GetMarkerNameFromTypeAndState(updateMessage.MarkerData.Type,
                                updateMessage.MarkerData.State)
                        })
                    {
                        Portal = portal,
                        Marker = updateMessage.MarkerData
                    };

                    if (updateMessage.MarkerData.Assignments.IsNotNullOrEmpty())
                        pin.Assignments = string.Join(", ", _agentsDatabase.GetAgents(updateMessage.MarkerData.Assignments).Result
                            .Select(x => x.Name).OrderBy(x => x));

                    Markers.Add(pin);
                    SelectedWasabeePin = pin;
                }

                _messenger.Publish(new MessageFrom<MapViewModel>(this, false));

                // If assigned to current user
                if (updateMessage.MarkerData.Assignments.IsNotNullOrEmpty() &&
                    updateMessage.MarkerData.Assignments.Contains(_userSettingsService.GetLoggedUserGoogleId()))
                    _messenger.Publish(new MessageFor<AssignmentsListViewModel>(this));
            }
        }

        private string GetMarkerNameFromTypeAndState(MarkerType markerType, TaskState markerState)
        {
            return $"{markerType.ToFriendlyString()} - {markerState.ToFriendlyString()}";
        }

        #endregion

        private readonly object _mapLayerConfigLock = new();
        private void UpdateMapLayersConfig()
        {
            lock (_mapLayerConfigLock)
            {
                var config = new MapLayersConfig()
                {
                    Links = IsLayerLinksActivated,
                    Markers = IsLayerMarkersActivated,
                    Anchors = IsLayerAnchorsActivated,
                    Agents = IsLayerAgentsActivated,
                    Zones = IsLayerZonesActivated
                };

                var raw = JsonConvert.SerializeObject(config);
                _preferences.Set(UserSettingsKeys.MapLayers, raw);
            }
        }
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

    class MapLayersConfig
    {
        public bool Links { get; set; }
        public bool Markers { get; set; }
        public bool Anchors { get; set; }
        public bool Agents { get; set; }
        public bool Zones { get; set; }
    }
}