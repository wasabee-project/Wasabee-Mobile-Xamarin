using Acr.UserDialogs;
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
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.Operation;
using Rocks.Wasabee.Mobile.Core.ViewModels.Profile;
using Rocks.Wasabee.Mobile.Core.ViewModels.Settings;
using Rocks.Wasabee.Mobile.Core.ViewModels.Teams;
using System;
using System.Linq;
using System.Threading.Tasks;
using Rocks.Wasabee.Mobile.Core.Services;
using Rocks.Wasabee.Mobile.Core.ViewModels.Dialogs;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Action = Rocks.Wasabee.Mobile.Core.Messages.Action;

#if DEBUG
using Rocks.Wasabee.Mobile.Core.ViewModels.Logs;
#endif

namespace Rocks.Wasabee.Mobile.Core.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly IAuthentificationService _authentificationService;
        private readonly IPreferences _preferences;
        private readonly IPermissions _permissions;
        private readonly IVersionTracking _versionTracking;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IUserDialogs _userDialogs;
        private readonly IMvxMessenger _messenger;
        private readonly OperationsDatabase _operationsDatabase;
        private readonly IDialogNavigationService _dialogNavigationService;

        private readonly MvxSubscriptionToken _token;

        public MenuViewModel(IMvxNavigationService navigationService, IAuthentificationService authentificationService,
            IPreferences preferences, IPermissions permissions, IVersionTracking versionTracking, IUserSettingsService userSettingsService,
            IUserDialogs userDialogs, IMvxMessenger messenger, OperationsDatabase operationsDatabase, IDialogNavigationService dialogNavigationService)
        {
            _navigationService = navigationService;
            _authentificationService = authentificationService;
            _preferences = preferences;
            _permissions = permissions;
            _versionTracking = versionTracking;
            _userSettingsService = userSettingsService;
            _userDialogs = userDialogs;
            _messenger = messenger;
            _operationsDatabase = operationsDatabase;
            _dialogNavigationService = dialogNavigationService;

            MenuItems = new MvxObservableCollection<MenuItem>()
            {
                new MenuItem() { Icon = "mdi-account", Title = "Profile", ViewModelType = typeof(ProfileViewModel) },
                new MenuItem() { Icon = "mdi-account-group", Title = "Teams", ViewModelType = typeof(TeamsListViewModel) },
                new MenuItem() { Icon = "mdi-map", Title = "Operation Map", ViewModelType = typeof(OperationRootTabbedViewModel) },
                new MenuItem() { Icon = "mdi-cogs", Title = "Settings", ViewModelType = typeof(SettingsViewModel) },
#if DEBUG
                new MenuItem() { Icon = "", Title = "", ViewModelType = null },
                new MenuItem() { Icon = "mdi-record", Title = "Live FCM Logs", ViewModelType = typeof(LogsViewModel) }
#endif
            };

            _token = messenger.Subscribe<NewOpAvailableMessage>(msg => RefreshAvailableOpsCommand.Execute());
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
                SelectedOpName = op == null ? "ERROR loading OP" : op.Name;
            }

            if (_preferences.Get(UserSettingsKeys.LiveLocationSharingEnabled, false))
            {
                _isLiveLocationSharingEnabled = true;
                _messenger.Publish(new LiveGeolocationTrackingMessage(this, Action.Start));
                await RaisePropertyChanged(() => IsLiveLocationSharingEnabled);
            }
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            RefreshAvailableOpsCommand.Execute();
        }

        #region Properties

        public string LoggedUser { get; set; } = string.Empty;
        public string DisplayVersion { get; set; } = string.Empty;
        public string SelectedOpName { get; set; } = string.Empty;
        public MvxObservableCollection<OperationModel> AvailableOpsCollection { get; set; } = new MvxObservableCollection<OperationModel>();
        public MvxObservableCollection<MenuItem> MenuItems { get; set; }
        public bool HasLocalOps { get; set; } = true;

        private MenuItem? _selectedMenuItem;
        public MenuItem? SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (SetProperty(ref _selectedMenuItem, value) && value != null)
                    SelectedMenuItemChangedCommand.Execute(value);
            }
        }

        private bool _isLiveLocationSharingEnabled;
        public bool IsLiveLocationSharingEnabled
        {
            get => _isLiveLocationSharingEnabled;
            set => ToggleLiveLocationSharingCommand.Execute(new Tuple<bool, bool>(value, false));
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
                ops.Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .OrderBy(x => x.Name));
        }

        public IMvxCommand<Tuple<bool, bool>> ToggleLiveLocationSharingCommand => new MvxCommand<Tuple<bool, bool>>(async value => await ToggleLiveLocationSharingExecuted(value.Item1, value.Item2));
        private async Task ToggleLiveLocationSharingExecuted(bool value, bool byPassWarning)
        {
            LoggingService.Trace($"Executing MenuViewModel.ToggleLiveLocationSharingCommand({value})");

            if (!_isLiveLocationSharingEnabled && value)
            {
                if (!_preferences.Get(UserSettingsKeys.NeverShowLiveLocationWaringAgain, false) && !byPassWarning)
                {
                    _ = _messenger.Subscribe<MessageFrom<LocationWarningDialogViewModel>>(msg =>
                    {
                        LoggingService.Trace("MenuViewModel - Activating location sharing from warning dialog");

                        if (msg.Data is bool neverShowWarningAgain && neverShowWarningAgain) {
                            _preferences.Set(UserSettingsKeys.NeverShowLiveLocationWaringAgain, true);
                            SetProperty(ref _isLiveLocationSharingEnabled, true, nameof(IsLiveLocationSharingEnabled));
                        }

                        ToggleLiveLocationSharingCommand.Execute(new Tuple<bool, bool>(value, true));
                    });
                    
                    LoggingService.Trace("MenuViewModel - Showing location warning dialog");
                    await _dialogNavigationService.Navigate<LocationWarningDialogViewModel>();
                }
                else
                {
                    if (await CheckAndAskForLocationPermissions())
                    {
                        SetProperty(ref _isLiveLocationSharingEnabled, true, nameof(IsLiveLocationSharingEnabled));
                        _preferences.Set(UserSettingsKeys.LiveLocationSharingEnabled, true);
                        _messenger.Publish(new LiveGeolocationTrackingMessage(this, Action.Start));
                    }
                }
            }
            else
            {
                SetProperty(ref _isLiveLocationSharingEnabled, false, nameof(IsLiveLocationSharingEnabled));
                _preferences.Set(UserSettingsKeys.LiveLocationSharingEnabled, false);
                _messenger.Publish(new LiveGeolocationTrackingMessage(this, Action.Stop));
            }
        }

        public IMvxCommand<MenuItem> SelectedMenuItemChangedCommand => new MvxCommand<MenuItem>(SelectedMenuItemChangedExecuted);
        private void SelectedMenuItemChangedExecuted(MenuItem menuItem)
        {
            LoggingService.Trace($"Executing MenuViewModel.SelectedMenuItemChangedCommand({menuItem.Title})");

            if (menuItem?.ViewModelType == null) return;

            _navigationService.Navigate(menuItem.ViewModelType);
            _selectedMenuItem = null;
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
            await _navigationService.Navigate<SplashScreenViewModel>();

            IsBusy = false;
        }

        public IMvxAsyncCommand PullOpsFromServerCommand => new MvxAsyncCommand(PullOpsFromServerExecuted);
        private async Task PullOpsFromServerExecuted()
        {
            LoggingService.Trace("Executing MenuViewModel.PullOpsFromServerCommand");

            if (IsBusy) return;
            IsBusy = true;

            await _navigationService.Navigate<SplashScreenViewModel, SplashScreenNavigationParameter>(new SplashScreenNavigationParameter(doDataRefreshOnly: true));

            IsBusy = false;
        }

        public IMvxCommand ChangeSelectedOpCommand => new MvxAsyncCommand(ChangeSelectedOpExecuted);
        private async Task ChangeSelectedOpExecuted()
        {
            LoggingService.Trace("Executing MenuViewModel.ChangeSelectedOpCommand");

            if (!HasLocalOps)
            {
                await _userDialogs.AlertAsync("No OP's available");
                return;
            }

            var opNames = AvailableOpsCollection.Select(x => x.Name).Except(new[] { SelectedOpName }).ToArray();
            var result = await _userDialogs.ActionSheetAsync("Available OP's :", "Cancel", null, null, opNames);
            if (string.IsNullOrWhiteSpace(result) || result.Equals("Cancel"))
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

        private async Task<bool> CheckAndAskForLocationPermissions()
        {
            var statusLocationAlways = await _permissions.CheckStatusAsync<Permissions.LocationAlways>();
            var statusLocationWhenInUse = await _permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (statusLocationAlways == PermissionStatus.Granted || statusLocationWhenInUse == PermissionStatus.Granted)
                return true;

            var requestResult = await _permissions.RequestAsync<Permissions.LocationAlways>();
            statusLocationWhenInUse = await _permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (requestResult != PermissionStatus.Granted && statusLocationWhenInUse != PermissionStatus.Granted)
            {
                _userDialogs.Alert("Geolocation permission is required !");
                return false;
            }

            LoggingService.Info("User has granted geolocation permissions");
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