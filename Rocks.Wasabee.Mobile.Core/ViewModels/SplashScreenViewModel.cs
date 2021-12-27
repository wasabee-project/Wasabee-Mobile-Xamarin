using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models;
using Rocks.Wasabee.Mobile.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using Rocks.Wasabee.Mobile.Core.QueryModels;
using Rocks.Wasabee.Mobile.Core.Resources.I18n;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.AgentVerification;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public class SplashScreenNavigationParameter
    {
        public bool DoDataRefreshOnly { get; }

        public SplashScreenNavigationParameter(bool doDataRefreshOnly)
        {
            DoDataRefreshOnly = doDataRefreshOnly;
        }
    }

    public class SplashScreenViewModel : BaseViewModel, IMvxViewModel<SplashScreenNavigationParameter>
    {
        private static int MessageDisplayTime => 150; // step message display timer in ms

        private readonly IConnectivity _connectivity;
        private readonly IPreferences _preferences;
        private readonly IVersionTracking _versionTracking;
        private readonly IAuthentificationService _authentificationService;
        private readonly IMvxNavigationService _navigationService;
        private readonly IMvxMessenger _messenger;
        private readonly ISecureStorage _secureStorage;
        private readonly IAppSettings _appSettings;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IUserDialogs _userDialogs;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly UsersDatabase _usersDatabase;
        private readonly OperationsDatabase _operationsDatabase;
        private readonly LinksDatabase _linksDatabase;
        private readonly MarkersDatabase _markersDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly TeamAgentsDatabase _teamAgentsDatabase;

        private bool _working = false;
        private bool _isBypassingGoogleAndWasabeeLogin = false;
        private bool _isUsingOneTimeToken = false;
        
        private int _tapCount = 0;

        private SplashScreenNavigationParameter? _parameter;

        public SplashScreenViewModel(IConnectivity connectivity, IPreferences preferences, IVersionTracking versionTracking,
            IAuthentificationService authentificationService, IMvxNavigationService navigationService, IMvxMessenger messenger,
            ISecureStorage secureStorage, IAppSettings appSettings, IUserSettingsService userSettingsService, IUserDialogs userDialogs,
            WasabeeApiV1Service wasabeeApiV1Service, UsersDatabase usersDatabase, OperationsDatabase operationsDatabase, LinksDatabase linksDatabase,
            MarkersDatabase markersDatabase, TeamsDatabase teamsDatabase, TeamAgentsDatabase teamAgentsDatabase)
        {
            _connectivity = connectivity;
            _preferences = preferences;
            _versionTracking = versionTracking;
            _authentificationService = authentificationService;
            _navigationService = navigationService;
            _messenger = messenger;
            _secureStorage = secureStorage;
            _appSettings = appSettings;
            _userSettingsService = userSettingsService;
            _userDialogs = userDialogs;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _usersDatabase = usersDatabase;
            _operationsDatabase = operationsDatabase;
            _linksDatabase = linksDatabase;
            _markersDatabase = markersDatabase;
            _teamsDatabase = teamsDatabase;
            _teamAgentsDatabase = teamAgentsDatabase;
        }

        public void Prepare(SplashScreenNavigationParameter parameter)
        {
            _parameter = parameter;
        }

        public override void Start()
        {
            base.Start();

            LoggingService.Trace("Starting SplashScreenViewModel");
            Analytics.TrackEvent(GetType().Name);

            AppEnvironnement = _preferences.Get(ApplicationSettingsConstants.AppEnvironnement, "unknown_env");
            var appVersion = _versionTracking.CurrentVersion;
            DisplayVersion = AppEnvironnement != "unknown_env" ? $"{AppEnvironnement} - v{appVersion}" : $"v{appVersion}";
        }

        public override Task Initialize()
        {
            LoggingService.Trace("Initializing SplashScreenViewModel");

            IsSettingButtonVisible = _preferences.Get(UserSettingsKeys.DevModeActivated, false);
            LoadCustomBackendServer();

            // TODO Handle app opening from notification


            LoadingStepLabel = Strings.SignIn_Label_LoadingStep_AppLoading;
            _connectivity.ConnectivityChanged += ConnectivityOnConnectivityChanged;

            RememberServerChoice = _preferences.Get(UserSettingsKeys.RememberServerChoice, false);

            return Task.CompletedTask;
        }

        public override async void ViewAppeared()
        {
            base.ViewAppeared();

            if (!_working)
            {
                _working = true;
                await StartApplication();
            }
        }

        #region Properties

        public bool IsLoading { get; set; }
        public bool IsConnected { get; set; }
        public bool IsLoginVisible { get; set; }
        public bool IsGButtonVisible { get; set; } = true;
        public bool IsOTTButtonVisible { get; set; } = true;
        public bool IsEnteringToken { get; set; }
        public bool IsAuthInError { get; set; }
        public bool IsSelectingServer { get; set; }
        public bool RememberServerChoice { get; set; }
        public bool HasNoTeamOrOpsAssigned { get; set; }
        public bool IsSettingButtonVisible { get; set; }
        public string LoadingStepLabel { get; set; } = string.Empty;
        public string AppEnvironnement { get; set; } = string.Empty;
        public string DisplayVersion { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string OneTimeToken { get; set; } = string.Empty;

        private ServerItem _selectedServerItem = ServerItem.Undefined;
        public ServerItem SelectedServerItem
        {
            get => _selectedServerItem;
            set
            {
                if (SetProperty(ref _selectedServerItem, value))
                {
                    if (SelectedServerItem.Server != WasabeeServer.Undefined)
                        _appSettings.Server = SelectedServerItem.Server;
                }
            }
        }

        private MvxObservableCollection<ServerItem> _serversCollection = new MvxObservableCollection<ServerItem>()
        {
            new("America", WasabeeServer.US, "US.png"),
            new("Europe", WasabeeServer.EU, "EU.png"),
            new("Asia/Pacific", WasabeeServer.APAC, "APAC.png")
        };
        public MvxObservableCollection<ServerItem> ServersCollection
        {
            get => _serversCollection;
            private set => SetProperty(ref _serversCollection, value);
        }

        #endregion

        #region Commands

        public IMvxAsyncCommand ConnectUserCommand => new MvxAsyncCommand(ConnectUser);
        private async Task ConnectUser()
        {
            LoggingService.Trace("Executing SplashScreenViewModel.ConnectUserCommand");

            if (!IsConnected)
            {
                IsLoading = false;
                IsLoginVisible = true;
                IsSelectingServer = false;

                return;
            }

            if (IsLoading) return;

            IsLoginVisible = false;
            IsLoading = true;
            LoadingStepLabel = Strings.SignIn_Label_LoadingStep_LoggingIn;

            await _usersDatabase.DeleteAllData();

            var savedServerChoice = _preferences.Get(UserSettingsKeys.SavedServerChoice, string.Empty);
            if (ServersCollection.Any(x => x.Server.ToString().Equals(savedServerChoice)))
                SelectedServerItem = ServersCollection.First(x => x.Server.ToString().Equals(savedServerChoice));

            var wtoken = await _secureStorage.GetAsync(SecureStorageConstants.WasabeeToken);
            if (!string.IsNullOrWhiteSpace(wtoken) && RememberServerChoice)
            {
                await BypassGoogleAndWasabeeLogin();
                return;
            }

            var token = await _authentificationService.GoogleLoginAsync();
            if (token != null)
            {
                await SaveGoogleToken(token);

                LoadingStepLabel = Strings.SignIn_Label_LoadingStep_GoogleSuccess;
                await Task.Delay(TimeSpan.FromMilliseconds(MessageDisplayTime));
                
                if (SelectedServerItem.Server == WasabeeServer.Undefined)
                    ChangeServerCommand.Execute();
                else
                    await ConnectWasabee();
            }
            else
            {
                ErrorMessage = Strings.SignIn_Label_ErrorMsg_GoogleError;
                IsAuthInError = true;
                IsLoginVisible = true;
            }

            IsLoading = false;
        }

        public IMvxCommand UseOneTimeTokenCommand => new MvxCommand(UseOneTimeToken);
        private void UseOneTimeToken()
        {
            LoggingService.Trace("Executing SplashScreenViewModel.UseOneTimeTokenCommand()");

            IsAuthInError = false;
            IsLoginVisible = false;

            if (SelectedServerItem != ServerItem.Undefined)
            {
                IsEnteringToken = true;
            }
            else
            {
                _isUsingOneTimeToken = true;
                
                IsEnteringToken = false;

                LoadingStepLabel = Strings.SignIn_Label_LoadingStep_SelectServerToken;
                IsSelectingServer = true;
            }
        }

        public IMvxAsyncCommand ValidateOneTimeTokenCommand => new MvxAsyncCommand(ValidateOneTimeToken);
        private async Task ValidateOneTimeToken()
        {
            LoggingService.Trace("Executing SplashScreenViewModel.ValidateOneTimeTokenCommand()");

            if (string.IsNullOrWhiteSpace(OneTimeToken))
                return;

            if (SelectedServerItem == ServerItem.Undefined)
            {
                _isUsingOneTimeToken = true;

                LoadingStepLabel = Strings.SignIn_Label_LoadingStep_SelectServerToken;
                IsLoginVisible = false;
                IsSelectingServer = true;
            }
            else
            {
                IsEnteringToken = false;
                IsSelectingServer = false;
                
                IsLoading = true;
                LoadingStepLabel = string.Format(Strings.SignIn_Label_LoadingStep_ContactingServer, SelectedServerItem.Name);

                var wasabeeUserModel = await _authentificationService.WasabeeOneTimeTokenLoginAsync(OneTimeToken);
                if (wasabeeUserModel != null)
                {
                    await _usersDatabase.SaveUserModel(wasabeeUserModel);

                    _userSettingsService.SaveLoggedUserGoogleId(wasabeeUserModel.GoogleId);
                    _userSettingsService.SaveIngressName(wasabeeUserModel.IngressName);

                    await FinishLogin(wasabeeUserModel, LoginMethod.OneTimeToken);
                }
                else
                {
                    _isUsingOneTimeToken = false;
                    
                    ErrorMessage = Strings.SignIn_Label_ErrorMsg_WasabeeFail;
                    IsAuthInError = true;
                    IsLoading = false;
                    IsLoginVisible = true;

                    OneTimeToken = string.Empty;
                    RememberServerChoice = false;
                    SelectedServerItem = ServerItem.Undefined;
                }
            }
        }

        public IMvxAsyncCommand<ServerItem> ChooseServerCommand => new MvxAsyncCommand<ServerItem>(ChooseServer);
        private async Task ChooseServer(ServerItem serverItem)
        {
            LoggingService.Trace($"Executing SplashScreenViewModel.ChooseServerCommand({serverItem})");

            IsSelectingServer = false;
            SelectedServerItem = serverItem;

            _preferences.Set(UserSettingsKeys.CurrentServer, SelectedServerItem.Server.ToString());

            if (_isBypassingGoogleAndWasabeeLogin)
            {
                _isBypassingGoogleAndWasabeeLogin = false;
                await BypassGoogleAndWasabeeLogin();
            }
            else if (_isUsingOneTimeToken)
            {
                UseOneTimeTokenCommand.Execute();
            }
            else
                await ConnectWasabee();
        }

        public IMvxCommand ChangeServerCommand => new MvxCommand(ChangeServer);
        private void ChangeServer()
        {
            LoggingService.Trace("Executing SplashScreenViewModel.ChangeServerCommand");

            HasNoTeamOrOpsAssigned = false;

            LoadingStepLabel = Strings.SignIn_Label_LoadingStep_SelectServer;
            IsLoading = false;
            IsLoginVisible = false;
            IsSelectingServer = true;
        }

        public IMvxAsyncCommand RetryTeamLoadingCommand => new MvxAsyncCommand(RetryTeamLoading);
        private async Task RetryTeamLoading()
        {
            LoggingService.Trace("Executing SplashScreenViewModel.RetryTeamLoadingCommand");

            if (IsLoading) return;

            HasNoTeamOrOpsAssigned = false;
            await ConnectWasabee();
        }

        public IMvxAsyncCommand ChangeAccountCommand => new MvxAsyncCommand(ChangeAccount);
        private async Task ChangeAccount()
        {
            LoggingService.Trace("Executing SplashScreenViewModel.ChangeAccountCommand");

            IsLoading = false;
            HasNoTeamOrOpsAssigned = false;
            IsLoginVisible = true;
            SelectedServerItem = ServerItem.Undefined;

            _preferences.Remove(UserSettingsKeys.RememberServerChoice);
            _preferences.Remove(UserSettingsKeys.SavedServerChoice);

            await ConnectUserCommand.ExecuteAsync();
        }

        public IMvxCommand RememberChoiceCommand => new MvxCommand(() =>
        {
            LoggingService.Trace("Executing SplashScreenViewModel.RememberChoiceCommand");

            RememberServerChoice = !RememberServerChoice;
        });

        public IMvxCommand ShowSettingCommand => new MvxCommand(ShowSettingExecuted);
        private async void ShowSettingExecuted()
        {
            LoggingService.Trace("Executing SplashScreenViewModel.ShowSettingCommand");

            var customBackendUri = string.Empty;
            var hasCustomBackendUri = _preferences.Get(UserSettingsKeys.HasCustomBackendUri, false);
            if (hasCustomBackendUri)
                customBackendUri = _preferences.Get(UserSettingsKeys.CustomBackendUri, string.Empty);

            var result = await _userDialogs.PromptAsync(customBackendUri, 
                Strings.Dialogs_Title_CustomServerUrl, 
                Strings.Global_Ok, 
                Strings.Global_Cancel);
            try
            {
                var value = result?.Text ?? string.Empty;
                if (value.StartsWith("http://"))
                    value = value.Replace("http", "https");

                var parsed = Uri.TryCreate(value, UriKind.Absolute, out Uri uriResult) && uriResult.Scheme == Uri.UriSchemeHttps;
                if (parsed)
                {
                    _preferences.Set(UserSettingsKeys.HasCustomBackendUri, true);
                    _preferences.Set(UserSettingsKeys.CustomBackendUri, value);

                    LoadCustomBackendServer();
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing SplashScreenViewModel.ShowSettingCommand");
            }
        }

        public IMvxCommand VersionTappedCommand => new MvxCommand(VersionTappedExecuted);
        private void VersionTappedExecuted()
        {
            LoggingService.Trace("Executing SplashScreenViewModel.VersionTappedCommand");

            if (IsSettingButtonVisible)
                return;

            _tapCount++;
            if (_tapCount < 5)
                return;

            IsSettingButtonVisible = true;
            _preferences.Set(UserSettingsKeys.DevModeActivated, true);

            _userDialogs.Toast(Strings.Global_DevModeActivated);

            LoggingService.Trace("SplashScreenViewModel : Dev mode activated");
        }

        #endregion

        #region Private methods

        private async void ConnectivityOnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            await StartApplication();
        }

        private async Task StartApplication()
        {
            IsConnected = _connectivity.ConnectionProfiles.Any() && _connectivity.NetworkAccess == NetworkAccess.Internet;

            if (_parameter != null && _parameter.DoDataRefreshOnly)
            {
                await BypassGoogleAndWasabeeLogin();

                return;
            }

            if (IsSelectingServer)
                ChangeServer();
            else
            {
                var lastLoginMethod = (LoginMethod) _preferences.Get(UserSettingsKeys.LastLoginMethod, (int) LoginMethod.Unknown);
                switch (lastLoginMethod)
                {
                    // will get a refreshed auth token from Google
                    case LoginMethod.Google:
                        await ConnectUserCommand.ExecuteAsync();
                        break;
                    // will use previous Cookie to bypass auth process
                    case LoginMethod.OneTimeToken:
                    case LoginMethod.Bypass:
                        await BypassGoogleAndWasabeeLogin();
                        break;
                    // user will choose login method
                    default:
                        IsLoginVisible = true;
                        break;
                }
            }
        }

        private async Task ConnectWasabee()
        {
            var token = await GetGoogleToken();
            if (token is null)
            {
                ErrorMessage = Strings.SignIn_Label_ErrorMsg_Internal;
                IsAuthInError = true;
                IsLoginVisible = true;
                IsLoading = false;
                return;
            }

            IsLoading = true;

            var savedServerChoice = _preferences.Get(UserSettingsKeys.SavedServerChoice, string.Empty);
            var currentServer = _preferences.Get(UserSettingsKeys.CurrentServer, string.Empty);

            if (ServersCollection.Any(x => x.Server.ToString().Equals(savedServerChoice)))
                SelectedServerItem = ServersCollection.First(x => x.Server.ToString().Equals(savedServerChoice));
            else if (ServersCollection.Any(x => x.Server.ToString().Equals(currentServer)))
                SelectedServerItem = ServersCollection.First(x => x.Server.ToString().Equals(currentServer));

            if (SelectedServerItem.Server == WasabeeServer.Undefined)
                ChangeServerCommand.Execute();
            else
            {
                LoadingStepLabel = string.Format(Strings.SignIn_Label_LoadingStep_ContactingServer, SelectedServerItem.Name);

                var wasabeeUserModel = await _authentificationService.WasabeeLoginAsync(token);
                if (wasabeeUserModel != null)
                {
                    await _usersDatabase.SaveUserModel(wasabeeUserModel);

                    _userSettingsService.SaveLoggedUserGoogleId(wasabeeUserModel!.GoogleId);
                    _userSettingsService.SaveIngressName(wasabeeUserModel!.IngressName);

                    await FinishLogin(wasabeeUserModel, LoginMethod.Google);
                }
                else
                {
                    ErrorMessage = Strings.SignIn_Label_ErrorMsg_WasabeeFail;
                    IsAuthInError = true;
                    IsLoading = false;
                    IsLoginVisible = true;

                    SelectedServerItem = ServerItem.Undefined;
                }
            }
        }

        private async Task BypassGoogleAndWasabeeLogin()
        {
            var wtoken = await _secureStorage.GetAsync(SecureStorageConstants.WasabeeToken);
            if (string.IsNullOrWhiteSpace(wtoken))
            {
                IsLoginVisible = true;
                IsLoading = false;

                RememberServerChoice = false;
                SelectedServerItem = ServerItem.Undefined;

                return;
            }

            _isBypassingGoogleAndWasabeeLogin = true;

            var savedServerChoice = _preferences.Get(UserSettingsKeys.SavedServerChoice, string.Empty);
            var currentServer = _preferences.Get(UserSettingsKeys.CurrentServer, string.Empty);

            if (ServersCollection.Any(x => x.Server.ToString().Equals(savedServerChoice)))
                SelectedServerItem = ServersCollection.First(x => x.Server.ToString().Equals(savedServerChoice));
            else if (ServersCollection.Any(x => x.Server.ToString().Equals(currentServer)))
                SelectedServerItem = ServersCollection.First(x => x.Server.ToString().Equals(currentServer));

            if (SelectedServerItem.Server == WasabeeServer.Undefined)
                ChangeServerCommand.Execute();
            else
            {
                IsLoading = true;
                LoadingStepLabel = string.Format(Strings.SignIn_Label_LoadingStep_ContactingServer, SelectedServerItem.Name);

                try
                {
                    var jwt = new JwtSecurityToken(wtoken);
                    var span = DateTime.UtcNow - jwt.IssuedAt;
                    if (span >= TimeSpan.FromHours(24))
                    {
                        var refreshedToken = await _wasabeeApiV1Service.User_RefreshWasabeeToken();
                        await _secureStorage.SetAsync(SecureStorageConstants.WasabeeToken, refreshedToken);
                    }

                    var userModel = await _wasabeeApiV1Service.User_GetUserInformations();
                    if (userModel != null)
                    {
                        await _usersDatabase.SaveUserModel(userModel);
                        await FinishLogin(userModel, LoginMethod.Bypass);
                    }
                    else
                        throw new NullReferenceException("SplashScreenViewModel.BypassGoogleAndWasabeeLogin() => _wasabeeApiV1Service.User_GetUserInformations() result is null");
                }
                catch
                {
                    // Auto login failed (expired token ?)
                    try
                    {
                        var lastLoginMethod = (LoginMethod) _preferences.Get(UserSettingsKeys.LastLoginMethod, (int) LoginMethod.Unknown);
                        if (lastLoginMethod == LoginMethod.OneTimeToken)
                            throw new Exception("Can't refresh Google token when Wasabee One Time Token was used previously");
                        
                        await ConnectWasabee();
                    }
                    catch (Exception e)
                    {
                        LoggingService.Error(e, "Error Executing SplashScreenViewModel.BypassGoogleAndWasabeeLogin");


                        ErrorMessage = Strings.SignIn_Label_ErrorMsg_WasabeeFail;
                        IsAuthInError = true;
                        IsLoading = false;

                        RememberServerChoice = false;
                        SelectedServerItem = ServerItem.Undefined;

                        await Task.Delay(TimeSpan.FromMilliseconds(MessageDisplayTime));

                        _isBypassingGoogleAndWasabeeLogin = false;
                        _secureStorage.Remove(SecureStorageConstants.WasabeeToken);

                        await ChangeAccountCommand.ExecuteAsync();
                    }
                }
            }
        }

        private async Task FinishLogin(UserModel userModel, LoginMethod loginMethod)
        {
            if (userModel.Blacklisted || userModel.IntelFaction.Equals("RESISTANCE"))
            {
                ErrorMessage = Strings.SignIn_Label_ErrorMsg_Internal;
                IsAuthInError = true;
                IsLoginVisible = true;
                IsGButtonVisible = false;
                IsOTTButtonVisible = false;
                IsLoading = false;
                return;
            }

            _messenger.Publish(new UserLoggedInMessage(this));
            _preferences.Set(UserSettingsKeys.LastLoginMethod, (int) loginMethod);
            
            if (RememberServerChoice)
            {
                _preferences.Set(UserSettingsKeys.RememberServerChoice, RememberServerChoice);
                _preferences.Set(UserSettingsKeys.SavedServerChoice, SelectedServerItem.Server.ToString());
            }
            else
            {
                _preferences.Remove(UserSettingsKeys.RememberServerChoice);
                _preferences.Remove(UserSettingsKeys.SavedServerChoice);
            }

            LoadingStepLabel = string.Format(Strings.SignIn_Label_LoadingStep_WelcomeAgent, userModel.IngressName);
            await Task.Delay(TimeSpan.FromMilliseconds(MessageDisplayTime * 2));

            try
            {
                if ((userModel.Teams == null || !userModel.Teams.Any()) &&
                    (userModel.Ops == null || !userModel.Ops.Any()))
                {
                    IsLoading = false;
                    HasNoTeamOrOpsAssigned = true;
                }
                else
                {
                    await PullDataFromServer(userModel).ContinueWith(async task =>
                    {
                        var shouldGoToAgentCommunityVerification = false;
                        if (string.IsNullOrEmpty(userModel.CommunityName))
                        {
                            shouldGoToAgentCommunityVerification = true;

                            if (_preferences.ContainsKey(UserSettingsKeys.NeverShowAgentCommunityVerificationAgain))
                            {
                                var dontAskAgainRawConfig = _preferences.Get(UserSettingsKeys.NeverShowAgentCommunityVerificationAgain, string.Empty);
                                if (string.IsNullOrEmpty(dontAskAgainRawConfig) is false)
                                {
                                    var config = JsonConvert.DeserializeObject<DontAskAgainConfig>(dontAskAgainRawConfig);
                                    if (config is not null)
                                    {
                                        var loggedUserId = _userSettingsService.GetLoggedUserGoogleId();
                                        if (config.Values.ContainsKey(loggedUserId))
                                            shouldGoToAgentCommunityVerification = !config.Values[loggedUserId];
                                    }
                                }
                            }
                        }
                        
                        await _navigationService.Navigate(Mvx.IoCProvider.Resolve<RootViewModel>());

                        if (shouldGoToAgentCommunityVerification)
                            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<AgentVerificationViewModel>(), 
                                new AgentVerificationNavigationParameter(comingFromLogin: true));
                    });

                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing SplashScreenViewModel.FinishLogin");

                IsAuthInError = true;
                IsLoginVisible = true;
                IsLoading = false;
                ErrorMessage = Strings.SignIn_Label_ErrorMsg_LoadingOpsData;
            }
        }

        private async Task PullDataFromServer(UserModel userModel)
        {
            LoadingStepLabel = Strings.SignIn_Label_LoadingStep_LoadingData + "\r\n" +
                               Strings.SignIn_Label_PleaseWait;

            await _teamsDatabase.DeleteAllData();
            await _teamAgentsDatabase.DeleteAllData();

            if (userModel.Teams != null && userModel.Teams.Any())
            {
                var teamIds = userModel.Teams.Select(t => t.Id).ToList();

                if (teamIds.Count == 1)
                {
                    var team = await _wasabeeApiV1Service.Teams_GetTeam(teamIds.First());
                    if (team != null)
                        await _teamsDatabase.SaveTeamModel(team);
                }
                else
                {
                    var teams = await _wasabeeApiV1Service.Teams_GetTeams(new GetTeamsQuery(teamIds));
                    if (teams.Any())
                        await _teamsDatabase.SaveTeamsModels(teams);
                }
            }

            if (userModel.Ops != null && userModel.Ops.Any())
            {
                var opsIds = userModel.Ops
                    .Select(x => x.Id)
                    .ToList();

                var selectedOp = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
                if (selectedOp == string.Empty || opsIds.All(id => id != selectedOp))
                {
                    var id = opsIds.First();
                    _preferences.Set(UserSettingsKeys.SelectedOp, id);
                    selectedOp = id;
                }

                // Ensure last selected Operation is loaded before going any further
                var op = await _wasabeeApiV1Service.Operations_GetOperation(selectedOp);
                if (op != null)
                    await _operationsDatabase.SaveOperationModel(op);
                else
                {
                    // Operation doesn't exist anymore on server ?
                    _preferences.Set(UserSettingsKeys.SelectedOp, string.Empty);
                    selectedOp = string.Empty;
                }

                var opsToLoad = opsIds.Except(new[] { selectedOp }).ToList();
                if (opsToLoad.Any())
                {
                    _ = Task.Factory.StartNew(async () =>
                    {
                        _userDialogs.Toast(Strings.Toasts_LoadingOpsInBackground);
                        
                        foreach (var id in opsToLoad)
                        {
                            var localOp = await _operationsDatabase.GetOperationModel(id);
                            var userModelOp = userModel.Ops.FirstOrDefault(x => x.Id.Equals(id));
                            if (userModelOp != null && localOp != null)
                            {
                                if (localOp.Modified.Equals(userModelOp.Modified) || localOp.LastEditID.Equals(userModelOp.LastEditID))
                                    continue;
                            }

                            op = await _wasabeeApiV1Service.Operations_GetOperation(id);
                            if (op != null)
                            {
                                // previously selected Operation can't be retrieved, set a new one as selected
                                if (string.IsNullOrEmpty(selectedOp))
                                {
                                    selectedOp = op.Id;
                                    _preferences.Set(UserSettingsKeys.SelectedOp, selectedOp);
                                }

                                await _operationsDatabase.SaveOperationModel(op);
                                _messenger.Publish(new NewOpAvailableMessage(this));
                            }
                        }

                        _userDialogs.Toast(Strings.Toasts_LoadingOpsSuccess);
                    }).ConfigureAwait(false);
                }
                else
                {
                    _preferences.Set(UserSettingsKeys.SelectedOp, string.Empty);
                }
            }
        }

        /// <summary>
        /// This will get the current GoogleToken saved in SecureStorage and verify if it's expired or not.
        /// If expired, it'll be automatically refreshed using the Refreshtoken in the GoogleToken.
        /// Else, it just returns as is.
        /// </summary>
        /// <returns>Current valid or new refreshed <see cref="GoogleToken"/></returns>
        private async Task<GoogleToken?> GetGoogleToken()
        {
            var rawGoogleToken = await _secureStorage.GetAsync(SecureStorageConstants.GoogleToken);
            if (!string.IsNullOrWhiteSpace(rawGoogleToken))
            {
                var googleToken = JsonConvert.DeserializeObject<GoogleToken>(rawGoogleToken);
                if (googleToken.CreatedAt.AddSeconds(double.Parse(googleToken.ExpiresIn)) <= DateTime.Now)
                {
                    var refreshedToken = await _authentificationService.RefreshGoogleTokenAsync(googleToken.RefreshToken);
                    if (refreshedToken != null)
                    {
                        googleToken = new GoogleToken()
                        {
                            AccessToken = refreshedToken.AccessToken,
                            ExpiresIn = refreshedToken.ExpiresIn,
                            Idtoken = refreshedToken.Idtoken,
                            Scope = refreshedToken.Scope,
                            TokenType = refreshedToken.Scope,
                            RefreshToken = googleToken.RefreshToken
                        };

                        await SaveGoogleToken(googleToken);
                    }
                }

                return googleToken;
            }

            return null;
        }

        private async Task SaveGoogleToken(GoogleToken? token)
        {
            await _secureStorage.SetAsync(SecureStorageConstants.GoogleToken, token != null ? JsonConvert.SerializeObject(token) : string.Empty);
        }

        private void LoadCustomBackendServer()
        {
            var hasCustomBackendUri = _preferences.Get(UserSettingsKeys.HasCustomBackendUri, false);
            if (!hasCustomBackendUri) 
                return;
            
            if (ServersCollection.Any(x => x.Server is WasabeeServer.Custom))
                ServersCollection.Remove(ServersCollection.First(x => x.Server is WasabeeServer.Custom));

            ServersCollection.Add(new ServerItem("Custom", WasabeeServer.Custom, ""));
            RaisePropertyChanged(nameof(ServersCollection));
        }

        #endregion

        private enum LoginMethod
        {
            Unknown = -1,
            Google,
            OneTimeToken,
            Bypass
        }
    }
}