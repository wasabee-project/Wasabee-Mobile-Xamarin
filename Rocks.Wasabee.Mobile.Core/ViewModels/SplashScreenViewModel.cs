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
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
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

            // TODO Handle app opening from notification


            LoadingStepLabel = "Application loading...";
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

        public MvxObservableCollection<ServerItem> ServersCollection => new MvxObservableCollection<ServerItem>()
        {
            new ServerItem("America", WasabeeServer.US, "US.png"),
            new ServerItem("Europe", WasabeeServer.EU, "EU.png"),
            new ServerItem("Asia/Pacific", WasabeeServer.APAC, "APAC.png")
        };

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
            LoadingStepLabel = "Logging in...";

            await _usersDatabase.DeleteAllData();

            var wasabeeCookie = await _secureStorage.GetAsync(SecureStorageConstants.WasabeeCookie);
            if (!string.IsNullOrWhiteSpace(wasabeeCookie) && RememberServerChoice)
            {
                await BypassGoogleAndWasabeeLogin();
                return;
            }

            var token = await _authentificationService.GoogleLoginAsync();
            if (token != null)
            {
                await SaveGoogleToken(token);

                LoadingStepLabel = "Google login success...";
                await Task.Delay(TimeSpan.FromMilliseconds(MessageDisplayTime));

                var savedServerChoice = _preferences.Get(UserSettingsKeys.SavedServerChoice, string.Empty);
                if (ServersCollection.Any(x => x.Server.ToString().Equals(savedServerChoice)))
                    SelectedServerItem = ServersCollection.First(x => x.Server.ToString().Equals(savedServerChoice));

                if (SelectedServerItem.Server == WasabeeServer.Undefined)
                    ChangeServerCommand.Execute();
                else
                    await ConnectWasabee();
            }
            else
            {
                ErrorMessage = "Google login failed !";
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

                LoadingStepLabel = "Choose token's server :";
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

                LoadingStepLabel = "Choose token's server :";
                IsLoginVisible = false;
                IsSelectingServer = true;
            }
            else
            {
                IsEnteringToken = false;
                IsSelectingServer = false;
                
                IsLoading = true;
                LoadingStepLabel = $"Contacting '{SelectedServerItem.Name}' Wasabee server...";

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
                    
                    ErrorMessage = "Wasabee login failed !";
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

            LoadingStepLabel = "Choose your server :";
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
                        await ConnectWasabee();
                        break;
                    // will use previous Cookie to bypass auth process
                    case LoginMethod.OneTimeToken:
                    case LoginMethod.Bypass:
                        await BypassGoogleAndWasabeeLogin();
                        break;
                    // use will choose
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
                ErrorMessage = "Internal error";
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
                LoadingStepLabel = $"Contacting '{SelectedServerItem.Name}' Wasabee server...";

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
                    ErrorMessage = "Wasabee login failed !";
                    IsAuthInError = true;
                    IsLoading = false;
                    IsLoginVisible = true;
                }
            }
        }

        private async Task BypassGoogleAndWasabeeLogin()
        {
            var cookie = await _secureStorage.GetAsync(SecureStorageConstants.WasabeeCookie);
            if (string.IsNullOrWhiteSpace(cookie))
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
                LoadingStepLabel = $"Contacting '{SelectedServerItem.Name}' Wasabee server...";

                try
                {
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
                    // Auto login failed (expired cookie ?)
                    // force relogin using saved googleToken if exist. If google token is expired, it will refresh it
                    try
                    {
                        var lastLoginMethod = (LoginMethod) _preferences.Get(UserSettingsKeys.LastLoginMethod, (int) LoginMethod.Unknown);
                        if (lastLoginMethod == LoginMethod.OneTimeToken)
                            throw new Exception("Can't refresh Google token when Wasabee One Time Token was used previously");

                        var token = await GetGoogleToken();
                        await SaveGoogleToken(token);

                        await ConnectWasabee();
                    }
                    catch (Exception e)
                    {
                        LoggingService.Error(e, "Error Executing SplashScreenViewModel.BypassGoogleAndWasabeeLogin");


                        ErrorMessage = "Wasabee login failed !";
                        IsAuthInError = true;
                        IsLoading = false;

                        RememberServerChoice = false;
                        SelectedServerItem = ServerItem.Undefined;

                        await Task.Delay(TimeSpan.FromMilliseconds(MessageDisplayTime));

                        _isBypassingGoogleAndWasabeeLogin = false;
                        _secureStorage.Remove(SecureStorageConstants.WasabeeCookie);

                        await ChangeAccountCommand.ExecuteAsync();
                    }
                }
            }
        }

        private async Task FinishLogin(UserModel userModel, LoginMethod loginMethod)
        {
            if (userModel.Blacklisted)
            {
                ErrorMessage = "Error occured";
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

            LoadingStepLabel = $"Welcome {userModel.IngressName}";
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
                    await PullDataFromServer(userModel)
                        .ContinueWith(async task =>
                        {
                            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<RootViewModel>());
                        });

                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing SplashScreenViewModel.FinishLogin");

                IsAuthInError = true;
                IsLoginVisible = true;
                IsLoading = false;
                ErrorMessage = "Error loading Wasabee OPs data";
            }
        }

        private async Task PullDataFromServer(UserModel userModel)
        {
            LoadingStepLabel = "Harvesting beehive,\r\n" +
                               "Please wait...";

            await _teamsDatabase.DeleteAllData();
            await _teamAgentsDatabase.DeleteAllData();
            await _operationsDatabase.DeleteAllData();

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

                _ = Task.Factory.StartNew(async () =>
                {
                    _userDialogs.Toast("Your OPs are loading in background");

                    foreach (var id in opsIds.Except(new[] { selectedOp }))
                    {
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

                    _userDialogs.Toast("OPs loaded succesfully");
                }).ConfigureAwait(false);
            }
            else
            {
                _preferences.Set(UserSettingsKeys.SelectedOp, string.Empty);
            }
        }

        private async Task<GoogleToken?> GetGoogleToken()
        {
            var rawGoogleToken = await _secureStorage.GetAsync(SecureStorageConstants.GoogleToken);
            if (!string.IsNullOrWhiteSpace(rawGoogleToken))
            {
                var googleToken = JsonConvert.DeserializeObject<GoogleToken>(rawGoogleToken);
                if (googleToken.CreatedAt.AddSeconds(double.Parse(googleToken.ExpiresIn)) <= DateTime.Now)
                {
                    var refreshedToken = await _authentificationService.RefreshTokenAsync(googleToken.RefreshToken);
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