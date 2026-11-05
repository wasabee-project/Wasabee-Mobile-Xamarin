using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rocks.Wasabee.Core.Adapters;
using Rocks.Wasabee.Core.Infra.Constants;
using Rocks.Wasabee.Core.Infra.Databases;
using Rocks.Wasabee.Core.Infra.Security;
using Rocks.Wasabee.Core.Models;
using Rocks.Wasabee.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Core.Models.Users;
using Rocks.Wasabee.Core.QueryModel;
using Rocks.Wasabee.Core.Resources.I18n;
using Rocks.Wasabee.Core.Services;
using Rocks.Wasabee.Core.Settings.Application;
using Rocks.Wasabee.Core.Settings.User;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Rocks.Wasabee.Core.ViewModels
{
    public partial class LoginPageViewModel : BaseViewModel
    {
        private static int MessageDisplayTime => 150; // step message display timer in ms

        private readonly IAppSettings _appSettings;
        private readonly IAuthentificationService _authentificationService;
        private readonly ISecureStorageAdapter _secureStorage;
        private readonly IPreferencesAdapter _preferences;
        private readonly IToastAdapter _toast;
        private readonly IUserSettingsService _userSettingsService;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly UsersDatabase _usersDatabase;
        private readonly AgentsDatabase _agentsDatabase;
        private readonly TeamsDatabase _teamsDatabase;
        private readonly OperationsDatabase _operationsDatabase;

        private bool _isBypassingGoogleAndWasabeeLogin = false;
        private bool _isUsingOneTimeToken = false;

        public LoginPageViewModel(
            IAppSettings appSettings,
            IAuthentificationService authentificationService,
            ISecureStorageAdapter secureStorage,
            IPreferencesAdapter preferences,
            IToastAdapter toast,
            IUserSettingsService userSettingsService,
            WasabeeApiV1Service wasabeeApiV1Service,
            UsersDatabase usersDatabase,
            AgentsDatabase agentsDatabase,
            TeamsDatabase teamsDatabase,
            OperationsDatabase operationsDatabase)
        {
            _appSettings = appSettings;
            _authentificationService = authentificationService;
            _secureStorage = secureStorage;
            _preferences = preferences;
            _toast = toast;
            _userSettingsService = userSettingsService;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _usersDatabase = usersDatabase;
            _agentsDatabase = agentsDatabase;
            _teamsDatabase = teamsDatabase;
            _operationsDatabase = operationsDatabase;
        }

        #region Properties

        [ObservableProperty] public partial bool IsLoading { get; set; } = false;
        [ObservableProperty] public partial bool IsConnected { get; set; } = true;
        [ObservableProperty] public partial bool IsLoginVisible { get; set; } = true;
        [ObservableProperty] public partial bool IsGButtonVisible { get; set; } = true;
        [ObservableProperty] public partial bool IsOTTButtonVisible { get; set; } = true;
        [ObservableProperty] public partial bool IsEnteringToken { get; set; }
        [ObservableProperty] public partial bool IsAuthInError { get; set; }
        [ObservableProperty] public partial bool IsSelectingServer { get; set; }
        [ObservableProperty] public partial bool RememberServerChoice { get; set; }
        [ObservableProperty] public partial bool HasNoTeamOrOpsAssigned { get; set; }
        [ObservableProperty] public partial bool IsSettingButtonVisible { get; set; }
        [ObservableProperty] public partial string LoadingStepLabel { get; set; } = string.Empty;
        [ObservableProperty] public partial string AppEnvironnement { get; set; } = string.Empty;
        [ObservableProperty] public partial string DisplayVersion { get; set; } = string.Empty;
        [ObservableProperty] public partial string ErrorMessage { get; set; } = string.Empty;
        [ObservableProperty] public partial string OneTimeToken { get; set; } = string.Empty;

        private ServerItem _selectedServerItem = ServerItem.Undefined;
        public ServerItem SelectedServerItem
        {
            get => _selectedServerItem;
            set
            {
                if (SetProperty(ref _selectedServerItem, value))
                    _appSettings.Server = SelectedServerItem.Server;
            }
        }

        public ObservableCollection<ServerItem> ServersCollection { get; } =
        [
            new("America", WasabeeServer.US, "flag_us.png"),
            new("Europe", WasabeeServer.EU, "flag_eu.png"),
            new("Asia/Pacific", WasabeeServer.APAC, "flag_apac.png")
        ];

        #endregion

        #region Commands

        [RelayCommand]
        public void RememberChoice()
        {
            RememberServerChoice = !RememberServerChoice;
        }

        [RelayCommand]
        private async Task ConnectUserAsync()
        {
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
            if (!string.IsNullOrWhiteSpace(wtoken))
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
                    ChangeServerCommand.Execute(null);
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

        [RelayCommand]
        private void UseOneTimeToken()
        {
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

        [RelayCommand]
        private void CancelOneTimeToken()
        {
            IsLoading = false;
            IsLoginVisible = true;
            IsEnteringToken = false;
            IsSelectingServer = false;
            IsAuthInError = false;

            SelectedServerItem = ServerItem.Undefined;

            OneTimeToken = string.Empty;
            LoadingStepLabel = string.Empty;
        }

        [RelayCommand]
        private async Task ValidateOneTimeToken()
        {
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
                    _userSettingsService.SaveIngressName(wasabeeUserModel.Name);

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

        [RelayCommand]
        private async Task ChooseServer(ServerItem serverItem)
        {
            IsSelectingServer = false;
            SelectedServerItem = serverItem;

            _preferences.Set(UserSettingsKeys.CurrentServer, SelectedServerItem.Server.ToString());

            if (_isBypassingGoogleAndWasabeeLogin)
            {
                _isBypassingGoogleAndWasabeeLogin = false;
                await BypassGoogleAndWasabeeLogin();
            }
            else if (_isUsingOneTimeToken)
                UseOneTimeTokenCommand.Execute(null);
            else
                await ConnectWasabee();
        }

        [RelayCommand]
        private void ChangeServer()
        {
            HasNoTeamOrOpsAssigned = false;

            LoadingStepLabel = Strings.SignIn_Label_LoadingStep_SelectServer;
            IsLoading = false;
            IsLoginVisible = false;
            IsSelectingServer = true;
        }

        [RelayCommand]
        private async Task RetryTeamLoading()
        {
            if (IsLoading) return;

            HasNoTeamOrOpsAssigned = false;
            await ConnectWasabee();
        }

        [RelayCommand]
        private void ChangeAccount()
        {
            IsLoading = false;
            HasNoTeamOrOpsAssigned = false;
            IsLoginVisible = true;

            _preferences.Remove(UserSettingsKeys.RememberServerChoice);
            _preferences.Remove(UserSettingsKeys.SavedServerChoice);

            _secureStorage.Remove(SecureStorageConstants.GoogleToken);
            _secureStorage.Remove(SecureStorageConstants.WasabeeToken);
        }

        #endregion

        #region Private methods

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

            if (ServersCollection.Any(x => x.Server.ToString().Equals(savedServerChoice)))
                SelectedServerItem = ServersCollection.First(x => x.Server.ToString().Equals(savedServerChoice));

            if (SelectedServerItem.Server == WasabeeServer.Undefined)
                ChangeServerCommand.Execute(null);
            else
            {
                LoadingStepLabel = string.Format(Strings.SignIn_Label_LoadingStep_ContactingServer, SelectedServerItem.Name);

                var wasabeeUserModel = await _authentificationService.WasabeeLoginAsync(token);
                if (wasabeeUserModel != null)
                {
                    await _usersDatabase.SaveUserModel(wasabeeUserModel);

                    _userSettingsService.SaveLoggedUserGoogleId(wasabeeUserModel.GoogleId);
                    _userSettingsService.SaveIngressName(wasabeeUserModel.Name);

                    await FinishLogin(wasabeeUserModel, LoginMethod.Google);
                }
                else
                {
                    ErrorMessage = "Wasabee login failed";
                    IsAuthInError = true;
                    IsLoading = false;
                    IsLoginVisible = true;
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
            if (ServersCollection.Any(x => x.Server.ToString().Equals(savedServerChoice)))
                SelectedServerItem = ServersCollection.First(x => x.Server.ToString().Equals(savedServerChoice));

            if (SelectedServerItem.Server == WasabeeServer.Undefined)
                ChangeServerCommand.Execute(null);
            else
            {
                IsLoading = true;
                LoadingStepLabel = string.Format(Strings.SignIn_Label_LoadingStep_ContactingServer, SelectedServerItem.Name);

                await Task.Delay(MessageDisplayTime);

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
                        var lastLoginMethod = (LoginMethod)_preferences.Get(UserSettingsKeys.LastLoginMethod, (int)LoginMethod.Unknown);
                        if (lastLoginMethod == LoginMethod.OneTimeToken)
                            throw new Exception("Can't refresh Google token when Wasabee One Time Token was used previously");

                        await ConnectWasabee();
                    }
                    catch (Exception)
                    {
                        ErrorMessage = Strings.SignIn_Label_ErrorMsg_WasabeeFail;
                        IsAuthInError = true;
                        IsLoading = false;

                        RememberServerChoice = false;
                        SelectedServerItem = ServerItem.Undefined;

                        await Task.Delay(TimeSpan.FromMilliseconds(MessageDisplayTime));

                        _isBypassingGoogleAndWasabeeLogin = false;
                        _secureStorage.Remove(SecureStorageConstants.WasabeeToken);

                        ChangeAccountCommand.Execute(null);
                    }
                }
            }
        }

        private async Task FinishLogin(UserModel userModel, LoginMethod loginMethod)
        {
            if (userModel.Blacklisted || userModel.Smurf || userModel.IntelFaction.Equals("RESISTANCE"))
            {
                ErrorMessage = Strings.SignIn_Label_ErrorMsg_Internal;
                IsAuthInError = true;
                IsLoginVisible = true;
                IsGButtonVisible = false;
                IsOTTButtonVisible = false;
                IsLoading = false;
                return;
            }

            //_messenger.Publish(new UserLoggedInMessage(this));
            _preferences.Set(UserSettingsKeys.LastLoginMethod, (int)loginMethod);

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

            LoadingStepLabel = string.Format(Strings.SignIn_Label_LoadingStep_WelcomeAgent, userModel.Name);
            await Task.Delay(TimeSpan.FromMilliseconds(MessageDisplayTime * 4));

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
                    await PullDataFromServer(userModel).ContinueWith(_ =>
                    {
                        //var shouldGoToTelegramLinking = false;
                        //if (SelectedServerItem.Server is not WasabeeServer.Undefined)
                        //    if (userModel.Telegram is null || userModel.Telegram.Verified is false)
                        //        shouldGoToTelegramLinking = true;

                        //await _navigationService.Navigate(Mvx.IoCProvider.Resolve<RootViewModel>());

                        //if (shouldGoToTelegramLinking)
                        //{
                        //    await _navigationService.Navigate<TelegramLinkingViewModel, TelegramLinkingNavigationParameter, TelegramLinkingCloseResult>(
                        //        new TelegramLinkingNavigationParameter(userModel));
                        //}
                    });

                }
            }
            catch (Exception e)
            {
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
            await _agentsDatabase.DeleteAllData();

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
                {
                    await _operationsDatabase.SaveOperationModel(op);
                }
                else
                {
                    // Operation doesn't exist anymore on server ?
                    _preferences.Set(UserSettingsKeys.SelectedOp, string.Empty);
                    selectedOp = string.Empty;
                }

                var allOps = opsIds.Except([selectedOp]).ToList();
                var upToDateOps = new List<string>();
                foreach (var id in allOps)
                {
                    var localOp = await _operationsDatabase.GetOperationModel(id);
                    var userModelOp = userModel.Ops.FirstOrDefault(x => x.Id.Equals(id));
                    if (userModelOp == null || localOp == null)
                        continue;

                    if (localOp.Modified.Equals(userModelOp.Modified) || localOp.LastEditId.Equals(userModelOp.LastEditId))
                    {
                        upToDateOps.Add(id);
                    }
                }

                var opsToLoad = allOps.Except(upToDateOps).ToList();
                if (opsToLoad.Any())
                {
                    _ = Task.Factory.StartNew(async () =>
                    {
                        _toast.Show(Strings.Toasts_LoadingOpsInBackground);

                        foreach (var id in opsToLoad)
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
                                //_messenger.Publish(new NewOpAvailableMessage(this));
                            }
                        }

                        _toast.Show(Strings.Toasts_LoadingOpsSuccess);
                    }).ConfigureAwait(false);
                }
                else
                {
                    _toast.Show(Strings.Toasts_LoadingOpsSuccess);
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
                var googleToken = JsonSerializer.Deserialize<GoogleToken>(rawGoogleToken);
                if (googleToken!.CreatedAt.AddSeconds(googleToken.ExpiresIn) <= DateTime.Now)
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
            await _secureStorage.SetAsync(SecureStorageConstants.GoogleToken, token != null ? JsonSerializer.Serialize(token) : string.Empty);
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
