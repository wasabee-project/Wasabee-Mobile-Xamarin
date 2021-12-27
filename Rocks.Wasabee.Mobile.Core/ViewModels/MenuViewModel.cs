using Acr.UserDialogs;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Constants;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Resources.I18n;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.Settings.Application;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs;
using Rocks.Wasabee.Mobile.Core.ViewModels.Logs;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation.Management;
using Rocks.Wasabee.Mobile.Core.ViewModels.Profile;
using Rocks.Wasabee.Mobile.Core.ViewModels.Settings;
using Rocks.Wasabee.Mobile.Core.ViewModels.Teams;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Action = Rocks.Wasabee.Mobile.Core.Messages.Action;

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly IAuthentificationService _authentificationService;
        private readonly IPreferences _preferences;
        private readonly IVersionTracking _versionTracking;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxMessenger _messenger;
        private readonly OperationsDatabase _operationsDatabase;
        private readonly UsersDatabase _usersDatabase;
        private readonly IDialogNavigationService _dialogNavigationService;
        private readonly IAppSettings _appSettings;

        private MvxSubscriptionToken? _token;
        private MvxSubscriptionToken? _tokenDebug;
        private MvxSubscriptionToken? _tokenOps;
        private MvxSubscriptionToken? _tokenStopGeolocationFromNotification;

        public MenuViewModel(IMvxNavigationService navigationService, IAuthentificationService authentificationService,
            IPreferences preferences, IVersionTracking versionTracking, IUserSettingsService userSettingsService,
            IUserDialogs userDialogs, IMvxMessenger messenger, OperationsDatabase operationsDatabase, UsersDatabase usersDatabase,
            IDialogNavigationService dialogNavigationService, IAppSettings appSettings)
        {
            _navigationService = navigationService;
            _authentificationService = authentificationService;
            _preferences = preferences;
            _versionTracking = versionTracking;
            _userSettingsService = userSettingsService;
            _userDialogs = userDialogs;
            _messenger = messenger;
            _operationsDatabase = operationsDatabase;
            _usersDatabase = usersDatabase;
            _dialogNavigationService = dialogNavigationService;
            _appSettings = appSettings;

            BuildMenu();
        }

        public override async void Prepare()
        {
            base.Prepare();

            var appEnvironnement = _preferences.Get(ApplicationSettingsConstants.AppEnvironnement, "unknown_env");
            var appVersion = _versionTracking.CurrentVersion;
            DisplayVersion = appEnvironnement != "unknown_env" ? $"{appEnvironnement} - v{appVersion}" : $"v{appVersion}";
            LoggedUser = _userSettingsService.GetIngressName();

            var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
            if (!string.IsNullOrWhiteSpace(selectedOpId))
            {
                var op = await _operationsDatabase.GetOperationModel(selectedOpId);
                SelectedOpName = op == null ? Strings.Menu_Label_ErrorLoadingOp : op.Name;
            }

            Server = _appSettings.Server switch
            {
                WasabeeServer.US => "America",
                WasabeeServer.EU => "Europe",
                WasabeeServer.APAC => "Asia/Pacific",
                WasabeeServer.Custom => "Custom",
                WasabeeServer.Undefined => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(Server))
            };
        }
        
        public override void ViewAppeared()
        {
            base.ViewAppeared();

            _token ??= _messenger.Subscribe<NewOpAvailableMessage>(msg => RefreshAvailableOpsCommand.Execute());
            _tokenOps ??= _messenger.Subscribe<MessageFrom<OperationsListViewModel>>(msg => RefreshAvailableOpsCommand.Execute());
            _tokenDebug ??= _messenger.SubscribeOnMainThread<MessageFrom<SettingsViewModel>>(msg =>
            {
                if (MenuItems.Any(x => x.ViewModelType == typeof(LogsViewModel)))
                    return;

                MenuItems.Add(new MenuItem() { Icon = "mdi-record", Title = "Live FCM Logs", ViewModelType = typeof(LogsViewModel) });
                RaisePropertyChanged(() => MenuItems);

                _preferences.Set(UserSettingsKeys.DevModeActivated, true);
            });
            _tokenStopGeolocationFromNotification ??= _messenger.Subscribe<LiveGeolocationTrackingMessage>(msg =>
            {
                if (msg.Sender != this && msg.Action == Action.Stop)
                    ToggleLiveLocationSharingCommand.Execute(false);
            });
            
            if (_preferences.Get(UserSettingsKeys.LiveLocationSharingEnabled, false))
            {
                SetProperty(ref _isLiveLocationSharingEnabled, true, nameof(IsLiveLocationSharingEnabled));
                _messenger.Publish(new LiveGeolocationTrackingMessage(this, Action.Start));
            }
            else
            {
                SetProperty(ref _isLiveLocationSharingEnabled, false, nameof(IsLiveLocationSharingEnabled));
                _messenger.Publish(new LiveGeolocationTrackingMessage(this, Action.Stop));
            }

            RefreshAvailableOpsCommand.Execute();
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();

            _token?.Dispose();
            _token = null;
            _tokenDebug?.Dispose();
            _tokenDebug = null;
            _tokenOps?.Dispose();
            _tokenOps = null;
            _tokenStopGeolocationFromNotification?.Dispose();
            _tokenStopGeolocationFromNotification = null;
        }

        #region Properties

        public string LoggedUser { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string DisplayVersion { get; set; } = string.Empty;
        public string SelectedOpName { get; set; } = string.Empty;
        public MvxObservableCollection<OperationModel> AvailableOpsCollection { get; set; } = new MvxObservableCollection<OperationModel>();
        public MvxObservableCollection<MenuItem> MenuItems { get; set; } = new MvxObservableCollection<MenuItem>();
        public bool HasLocalOps { get; set; } = true;

        private bool _isLiveLocationSharingEnabled;
        public bool IsLiveLocationSharingEnabled
        {
            get => _isLiveLocationSharingEnabled;
            set => ToggleLiveLocationSharingCommand.Execute(value);
        }

        #endregion

        #region Commands

        public IMvxCommand RefreshAvailableOpsCommand => new MvxCommand(async () => await RefreshAvailableOpsExecuted());
        private async Task RefreshAvailableOpsExecuted()
        {
            LoggingService.Trace("Executing MenuViewModel.RefreshAvailableOpsCommand");

            var ops = await _operationsDatabase.GetOperationModels();
            if (ops.IsNullOrEmpty())
            {
                HasLocalOps = false;
                return;
            }

            AvailableOpsCollection = new MvxObservableCollection<OperationModel>(
                ops.Where(x => !string.IsNullOrWhiteSpace(x.Name) && !x.IsHiddenLocally)
                .OrderBy(x => x.Name));
        }

        public IMvxCommand<bool> ToggleLiveLocationSharingCommand => new MvxCommand<bool>(async value => await ToggleLiveLocationSharingExecuted(value));
        private async Task ToggleLiveLocationSharingExecuted(bool value)
        {
            LoggingService.Trace($"Executing MenuViewModel.ToggleLiveLocationSharingCommand({value})");

            if (!_isLiveLocationSharingEnabled && value)
            {
                var teams = await _usersDatabase.GetUserTeams(_userSettingsService.GetLoggedUserGoogleId());
                if (teams.Any(x => x.State.Equals("On")) is false)
                {
                    var result = await _userDialogs.ConfirmAsync(
                        Strings.Dialog_Warning_NoTeamSharingEnabled,
                        string.Empty,
                        Strings.Dialog_Warning_GoToTeamsList,
                        Strings.Global_Cancel);

                    if (result)
                    {
                        await _navigationService.Navigate<TeamsListViewModel>();

                        // Message used to close menu UI
                        _messenger.Publish(new MessageFrom<MenuViewModel>(this));
                    }

                    return;
                }

                if (_preferences.Get(UserSettingsKeys.NeverShowLiveLocationWarningAgain, false) is false)
                {
                    LoggingService.Trace("MenuViewModel - Showing location warning dialog");
                    var result = await _dialogNavigationService.Navigate<LocationWarningDialogViewModel, LocationWarningDialogResult>();

                    if (result.NeverShowWarningAgain)
                        _preferences.Set(UserSettingsKeys.NeverShowLiveLocationWarningAgain, true);

                    if (!result.Accepted)
                        return;

                    LoggingService.Trace("MenuViewModel - Activating location sharing from warning dialog");
                }

                try
                {
                    if (await CheckAndAskForLocationPermissions())
                        StartLiveLocation();
                    else
                        StopLiveLocation();
                }
                catch (Exception e)
                {
                    LoggingService.Error(e, "Error MenuViewModel requesing permission LocationAlways");
                }
            }
            else
            {
                StopLiveLocation();
            }

            void StartLiveLocation()
            {
                SetProperty(ref _isLiveLocationSharingEnabled, true, nameof(IsLiveLocationSharingEnabled));
                _preferences.Set(UserSettingsKeys.LiveLocationSharingEnabled, true);
                _messenger.Publish(new LiveGeolocationTrackingMessage(this, Action.Start));

                if (Device.RuntimePlatform == Device.Android)
                    _messenger.Publish(new PromptAndroidRunInBackgroundMessage(this));
            }

            void StopLiveLocation()
            {
                SetProperty(ref _isLiveLocationSharingEnabled, false, nameof(IsLiveLocationSharingEnabled));
                _preferences.Set(UserSettingsKeys.LiveLocationSharingEnabled, false);
                _messenger.Publish(new LiveGeolocationTrackingMessage(this, Action.Stop));
            }
        }

        public IMvxCommand<MenuItem> SelectedMenuItemChangedCommand => new MvxCommand<MenuItem>(SelectedMenuItemChangedExecuted);
        private async void SelectedMenuItemChangedExecuted(MenuItem menuItem)
        {
            LoggingService.Trace($"Executing MenuViewModel.SelectedMenuItemChangedCommand({menuItem.Title})");

            if (menuItem.ViewModelType == null)
                return;

            await _navigationService.Navigate(Mvx.IoCProvider.Resolve(menuItem.ViewModelType) as IMvxViewModel);
        }

        public IMvxAsyncCommand LogoutCommand => new MvxAsyncCommand(Logout);
        private async Task Logout()
        {
            LoggingService.Trace("Executing MenuViewModel.LogoutCommand");

            if (IsBusy) return;
            IsBusy = true;

            if (IsLiveLocationSharingEnabled)
                IsLiveLocationSharingEnabled = false;

            await _authentificationService.LogoutAsync();

            await _navigationService.Close(this);
            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<SplashScreenViewModel>());

            IsBusy = false;
        }

        public IMvxAsyncCommand PullOpsFromServerCommand => new MvxAsyncCommand(PullOpsFromServerExecuted);
        private async Task PullOpsFromServerExecuted()
        {
            LoggingService.Trace("Executing MenuViewModel.PullOpsFromServerCommand");

            if (IsBusy) return;
            IsBusy = true;
            
            await _navigationService.Navigate(Mvx.IoCProvider.Resolve<SplashScreenViewModel>(), new SplashScreenNavigationParameter(doDataRefreshOnly: true));

            IsBusy = false;
        }

        public IMvxCommand ChangeSelectedOpCommand => new MvxAsyncCommand(ChangeSelectedOpExecuted);
        private async Task ChangeSelectedOpExecuted()
        {
            LoggingService.Trace("Executing MenuViewModel.ChangeSelectedOpCommand");

            if (!HasLocalOps)
            {
                await _userDialogs.AlertAsync(Strings.Dialog_Warning_NoOpsAvailable);
                return;
            }

            var opNames = AvailableOpsCollection.Select(x => x.Name).Except(new[] { SelectedOpName }).ToArray();
            var result = await _userDialogs.ActionSheetAsync(Strings.Dialogs_Title_AvailableOpsList, Strings.Global_Cancel, null, null, opNames);
            if (string.IsNullOrWhiteSpace(result) || result.Equals(Strings.Global_Cancel))
                return;

            var selectedOp = AvailableOpsCollection.FirstOrDefault(x => x.Name.Equals(result));
            if (selectedOp == null)
                return;

            SelectedOpName = selectedOp.Name;
            _preferences.Set(UserSettingsKeys.SelectedOp, selectedOp.Id);
            _messenger.Publish(new SelectedOpChangedMessage(this, selectedOp.Id));
        }

        #endregion

        #region Private methods

        private void BuildMenu()
        {
            MenuItems = new MvxObservableCollection<MenuItem>()
            {
                new MenuItem() { Icon = "mdi-account", Title = Strings.Menu_Item_Profile, ViewModelType = typeof(ProfileViewModel) },
                new MenuItem() { Icon = "mdi-account-group", Title = Strings.Menu_Item_Teams, ViewModelType = typeof(TeamsListViewModel) },
                new MenuItem() { Icon = "mdi-map", Title = Strings.Menu_Item_OperationMap, ViewModelType = typeof(OperationRootTabbedViewModel) },
                new MenuItem() { Icon = "mdi-widgets", Title = Strings.Menu_Item_OperationsList, ViewModelType = typeof(OperationsListViewModel) },
                new MenuItem() { Icon = "mdi-cogs", Title = Strings.Menu_Item_Settings, ViewModelType = typeof(SettingsViewModel) }
            };

            if (_preferences.Get(UserSettingsKeys.DevModeActivated, false))
                MenuItems.Add(new MenuItem() { Icon = "mdi-record", Title = "Live FCM Logs", ViewModelType = typeof(LogsViewModel) });
        }

        private async Task<bool> CheckAndAskForLocationPermissions()
        {
            LoggingService.Trace("MenuViewModel - Checking location permissions");
            
            var statusLocationAlways = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

            LoggingService.Trace($"Permissions Status : LocationAlways={statusLocationAlways}");

            if (statusLocationAlways == PermissionStatus.Granted)
                return true;
            
            var requestPermission = true;
            var showRationale = Permissions.ShouldShowRationale<Permissions.LocationAlways>();
            if (showRationale)
                requestPermission = await _userDialogs.ConfirmAsync(
                  Strings.Dialogs_Warning_SetLocationPermissionsAllTheTime,
                  Strings.Dialog_Warning_PermissionsRequired,
                  Strings.Global_Ok, Strings.Global_Cancel);

            if (!requestPermission)
                return false;

            LoggingService.Trace("MenuViewModel - Requesting location permissions");
            await Permissions.RequestAsync<Permissions.LocationAlways>();
            statusLocationAlways = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            LoggingService.Trace($"Permissions Status : LocationAlways={statusLocationAlways}");

            if (statusLocationAlways != PermissionStatus.Granted)
            {
                LoggingService.Trace("User didn't granted geolocation permissions");

                _userDialogs.Alert(Strings.Dialogs_Warning_LocationPermissionRequired);
                return false;
            }

            LoggingService.Info("MenuViewModel - User has granted geolocation permissions");
            return true;

        }

        #endregion
    }

    public class MenuItem
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public Type? ViewModelType { get; set; }
    }
}