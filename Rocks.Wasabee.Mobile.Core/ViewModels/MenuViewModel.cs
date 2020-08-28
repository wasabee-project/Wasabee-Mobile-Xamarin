using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Messages;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using Rocks.Wasabee.Mobile.Core.ViewModels.Logs;
using Rocks.Wasabee.Mobile.Core.ViewModels.Map;
using Rocks.Wasabee.Mobile.Core.ViewModels.Profile;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;
using Action = Rocks.Wasabee.Mobile.Core.Messages.Action;

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

        public MenuViewModel(IMvxNavigationService navigationService, IAuthentificationService authentificationService,
            IPreferences preferences, IPermissions permissions, IVersionTracking versionTracking, IUserSettingsService userSettingsService,
            IUserDialogs userDialogs, IMvxMessenger messenger, OperationsDatabase operationsDatabase)
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

            MenuItems = new MvxObservableCollection<MenuItem>()
            {
                new MenuItem() { Title = "Profile", ViewModelType = typeof(ProfileViewModel) },
                new MenuItem() { Title = "Operation Map", ViewModelType = typeof(MapViewModel) },
                new MenuItem() { Title = "Live Logs", ViewModelType = typeof(LogsViewModel) }
            };
        }

        public override async void Prepare()
        {
            base.Prepare();

            var appEnvironnement = _preferences.Get("appEnvironnement", "unknown_env");
            var appVersion = _versionTracking.CurrentVersion;
            DisplayVersion = appEnvironnement != "prod" ? $"{appEnvironnement} - v{appVersion}" : $"v{appVersion}";
            LoggedUser = _userSettingsService.GetIngressName();

            var selectedOpId = _preferences.Get(UserSettingsKeys.SelectedOp, string.Empty);
            if (!string.IsNullOrWhiteSpace(selectedOpId))
            {
                var op = await _operationsDatabase.GetOperationModel(selectedOpId);
                SelectedOpName = op.Name;
            }

            AvailableOpsCollection = new MvxObservableCollection<OperationModel>((await _operationsDatabase.GetOperationModels()).Where(x => !string.IsNullOrWhiteSpace(x.Name)));
        }

        #region Properties

        public string LoggedUser { get; set; }
        public string DisplayVersion { get; set; }
        public string SelectedOpName { get; set; }
        public MvxObservableCollection<OperationModel> AvailableOpsCollection { get; set; }
        public MvxObservableCollection<MenuItem> MenuItems { get; set; }

        private MenuItem _selectedMenuItem;
        public MenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (SetProperty(ref _selectedMenuItem, value))
                    SelectedMenuItemChangedCommand.Execute(value);
            }
        }

        private bool _isLiveLocationSharingEnabled;
        public bool IsLiveLocationSharingEnabled
        {
            get => _isLiveLocationSharingEnabled;
            set => ToggleLiveLocationSharingCommand.Execute(value);
        }

        #endregion

        #region Commands

        public IMvxCommand<bool> ToggleLiveLocationSharingCommand => new MvxCommand<bool>(async value => await ToggleLiveLocationSharingExecuted(value));
        private async Task ToggleLiveLocationSharingExecuted(bool value)
        {
            var statusLocationAlways = await _permissions.CheckStatusAsync<Permissions.LocationAlways>();
            var statusLocationWhenInUse = await _permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (statusLocationAlways != PermissionStatus.Granted || statusLocationWhenInUse != PermissionStatus.Granted)
            {
                var result = await _permissions.RequestAsync<Permissions.LocationAlways>();
                statusLocationWhenInUse = await _permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (result != PermissionStatus.Granted && statusLocationWhenInUse != PermissionStatus.Granted)
                {
                    _userDialogs.Alert("Geolocation permission is required !");
                    return;
                }
            }

            if (!_isLiveLocationSharingEnabled && value)
            {
                var result = await _userDialogs.ConfirmAsync(
                    "Your location will be shared with ALL your enabled teams. Start tracking anyway ?", "Warning",
                    "Yes", "No");

                if (!result)
                    return;

                SetProperty(ref _isLiveLocationSharingEnabled, true, nameof(IsLiveLocationSharingEnabled));
                _messenger.Publish(new LiveGeolocationTrackingMessage(this, Action.Start));
            }
            else
            {
                SetProperty(ref _isLiveLocationSharingEnabled, false, nameof(IsLiveLocationSharingEnabled));
                _messenger.Publish(new LiveGeolocationTrackingMessage(this, Action.Stop));
            }
        }

        public IMvxCommand<MenuItem> SelectedMenuItemChangedCommand => new MvxCommand<MenuItem>(SelectedMenuItemChangedExecuted);
        private void SelectedMenuItemChangedExecuted(MenuItem menuItem)
        {
            if (menuItem?.ViewModelType == null) return;

            _navigationService.Navigate(menuItem.ViewModelType);
            _selectedMenuItem = null;
        }

        public IMvxAsyncCommand LogoutCommand => new MvxAsyncCommand(Logout);
        private async Task Logout()
        {
            await _authentificationService.LogoutAsync();

            await _navigationService.Navigate<SplashScreenViewModel>();
        }

        public IMvxCommand ChangeSelectedOpCommand => new MvxAsyncCommand(ChangeSelectedOpExecuted);
        private async Task ChangeSelectedOpExecuted()
        {
            var result = await _userDialogs.ActionSheetAsync("Available OP's :", "Cancel", null, null, AvailableOpsCollection.Select(x => x.Name).ToArray());
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
    }

    public class MenuItem
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public Type ViewModelType { get; set; }
    }
}