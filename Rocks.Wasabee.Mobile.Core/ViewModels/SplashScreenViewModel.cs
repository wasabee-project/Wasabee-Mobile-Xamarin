using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models;
using Rocks.Wasabee.Mobile.Core.Models.Auth.Google;
using Rocks.Wasabee.Mobile.Core.Models.Auth.Wasabee;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public class SplashScreenViewModel : BaseViewModel
    {
        private readonly IConnectivity _connectivity;
        private readonly IPreferences _preferences;
        private readonly IVersionTracking _versionTracking;
        private readonly IAuthentificationService _authentificationService;
        private readonly IMvxNavigationService _navigationService;
        private readonly IAppSettings _appSettings;
        private readonly IUserSettingsService _userSettingsService;
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly OperationsDatabase _operationsDatabase;

        private bool _working;
        private GoogleToken _googleToken;

        public SplashScreenViewModel(IConnectivity connectivity, IPreferences preferences, IVersionTracking versionTracking,
            IAuthentificationService authentificationService, IMvxNavigationService navigationService,
            IAppSettings appSettings, IUserSettingsService userSettingsService, WasabeeApiV1Service wasabeeApiV1Service,
            OperationsDatabase operationsDatabase)
        {
            _connectivity = connectivity;
            _preferences = preferences;
            _versionTracking = versionTracking;
            _authentificationService = authentificationService;
            _navigationService = navigationService;
            _appSettings = appSettings;
            _userSettingsService = userSettingsService;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _operationsDatabase = operationsDatabase;
        }

        public override void Start()
        {
            base.Start();

            AppEnvironnement = _preferences.Get("appEnvironnement", "unknown_env");
            var appVersion = _versionTracking.CurrentVersion;
            DisplayVersion = AppEnvironnement != "prod" ? $"{AppEnvironnement} - v{appVersion}" : $"v{appVersion}";
        }

        public override Task Initialize()
        {
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
        public bool IsAuthInError { get; set; }
        public bool IsSelectingServer { get; set; }
        public bool RememberServerChoice { get; set; }
        public bool HasNoTeamOrOpsAssigned { get; set; }
        public string LoadingStepLabel { get; set; }
        public string AppEnvironnement { get; set; }
        public string DisplayVersion { get; set; }
        public string ErrorMessage { get; set; }

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

            await Task.Delay(TimeSpan.FromMilliseconds(300));
            _googleToken = await _authentificationService.GoogleLoginAsync();

            if (_googleToken != null)
            {
                LoadingStepLabel = "Google login success...";
                await Task.Delay(TimeSpan.FromMilliseconds(300));

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

        public IMvxAsyncCommand<ServerItem> ChooseServerCommand => new MvxAsyncCommand<ServerItem>(ChooseServer);
        private async Task ChooseServer(ServerItem serverItem)
        {
            IsSelectingServer = false;
            SelectedServerItem = serverItem;

            await ConnectWasabee();
        }

        public IMvxCommand ChangeServerCommand => new MvxCommand(ChangeServer);
        private void ChangeServer()
        {
            HasNoTeamOrOpsAssigned = false;

            LoadingStepLabel = "Choose your server :";
            IsLoginVisible = false;
            IsSelectingServer = true;
        }

        public IMvxAsyncCommand RetryTeamLoadingCommand => new MvxAsyncCommand(RetryTeamLoading);
        private async Task RetryTeamLoading()
        {
            if (IsLoading) return;

            HasNoTeamOrOpsAssigned = false;
            await ConnectWasabee();
        }

        public IMvxAsyncCommand ChangeAccountCommand => new MvxAsyncCommand(ChangeAccount);
        private async Task ChangeAccount()
        {
            IsLoading = false;
            HasNoTeamOrOpsAssigned = false;
            IsLoginVisible = true;
            SelectedServerItem = null;

            _preferences.Remove(UserSettingsKeys.RememberServerChoice);
            _preferences.Remove(UserSettingsKeys.SavedServerChoice);

            await ConnectUserCommand.ExecuteAsync();
        }

        public IMvxCommand RememberChoiceCommand => new MvxCommand(() => RememberServerChoice = !RememberServerChoice);

        #endregion

        #region Private methods

        private async void ConnectivityOnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            await StartApplication();
        }

        private async Task StartApplication()
        {
            IsConnected = _connectivity.ConnectionProfiles.Any() && _connectivity.NetworkAccess == NetworkAccess.Internet;

            await ConnectUserCommand.ExecuteAsync();
        }

        private async Task ConnectWasabee()
        {
            IsLoading = true;
            LoadingStepLabel = $"Contacting '{SelectedServerItem.Name}' Wasabee server...";
            await Task.Delay(TimeSpan.FromMilliseconds(300));

            var wasabeeLoginResponse = await _authentificationService.WasabeeLoginAsync(_googleToken);
            if (wasabeeLoginResponse != null)
            {
                Mvx.IoCProvider.Resolve<IMvxMessenger>().Publish(new UserLoggedInMessage(this));

                if (RememberServerChoice)
                {
                    _preferences.Set(UserSettingsKeys.RememberServerChoice, RememberServerChoice);
                    _preferences.Set(UserSettingsKeys.SavedServerChoice, SelectedServerItem.Server.ToString());
                }

                _userSettingsService.SaveIngressName(wasabeeLoginResponse.IngressName);

                await FinishLogin(wasabeeLoginResponse);
            }
            else
            {
                ErrorMessage = "Wasabee login failed !";
                IsAuthInError = true;
                IsLoading = false;
                IsLoginVisible = true;
            }
        }

        private async Task FinishLogin(WasabeeLoginResponse wasabeeLoginResponse)
        {
            LoadingStepLabel = $"Welcome {wasabeeLoginResponse.IngressName}";
            await Task.Delay(TimeSpan.FromSeconds(1));

            if ((wasabeeLoginResponse.Teams == null || !wasabeeLoginResponse.Teams.Any()) &&
                (wasabeeLoginResponse.OwnedTeams == null || !wasabeeLoginResponse.OwnedTeams.Any()) &&
                (wasabeeLoginResponse.Ops == null || !wasabeeLoginResponse.Ops.Any()) &&
                (wasabeeLoginResponse.OwnedOps == null || !wasabeeLoginResponse.OwnedOps.Any()))
            {
                IsLoading = false;
                HasNoTeamOrOpsAssigned = true;
            }
            else
            {
                LoadingStepLabel = "Loading OPs,\r\n" +
                                   "Please wait...";
                await Task.Delay(TimeSpan.FromMilliseconds(300));

                var opsIds = wasabeeLoginResponse.Ops?.Select(x => x.Id).ToList().Union(wasabeeLoginResponse.OwnedOps?.Select(x => x.Id).ToList() ?? new List<string>()) ?? new List<string>();
                foreach (var id in opsIds)
                {
                    var op = await _wasabeeApiV1Service.GetOperation(id);
                    if (op != null)
                    {
                        await _operationsDatabase.SaveOperationModel(op);
                    }
                }

                //_firebaseAnalyticsService.LogEvent("Login");

                await _navigationService.Navigate<RootViewModel>();
                await _navigationService.Close(this);
            }
        }

        #endregion
    }
}