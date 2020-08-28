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
using Xamarin.Essentials.Interfaces;

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

        private MenuItem _selectedMenuItem;

        public MenuViewModel(IMvxNavigationService navigationService, IAuthentificationService authentificationService,
            IPreferences preferences, IVersionTracking versionTracking, IUserSettingsService userSettingsService,
            IUserDialogs userDialogs, IMvxMessenger messenger, OperationsDatabase operationsDatabase)
        {
            _navigationService = navigationService;
            _authentificationService = authentificationService;
            _preferences = preferences;
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
        public MenuItem SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (SetProperty(ref _selectedMenuItem, value))
                    SelectedMenuItemChangedCommand.Execute(value);
            }
        }

        #endregion

        #region Commands

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