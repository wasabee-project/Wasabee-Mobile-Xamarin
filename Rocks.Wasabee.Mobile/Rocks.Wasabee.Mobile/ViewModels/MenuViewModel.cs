using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Security;
using Rocks.Wasabee.Mobile.Core.Settings.User;
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

        private MenuItem _selectedMenuItem;

        public MenuViewModel(IMvxNavigationService navigationService, IAuthentificationService authentificationService,
            IPreferences preferences, IVersionTracking versionTracking, IUserSettingsService userSettingsService)
        {
            _navigationService = navigationService;
            _authentificationService = authentificationService;
            _preferences = preferences;
            _versionTracking = versionTracking;
            _userSettingsService = userSettingsService;

            MenuItems = new MvxObservableCollection<MenuItem>()
            {
                new MenuItem() { Title = "Profile", ViewModelType = typeof(MenuViewModel) },
                new MenuItem() { Title = "OP's", Subtitle = "This is an info subtitle", ViewModelType = typeof(MenuViewModel) }
            };
        }

        public override void Prepare()
        {
            base.Prepare();

            var appEnvironnement = _preferences.Get("appEnvironnement", "unknown_env");
            var appVersion = _versionTracking.CurrentVersion;
            DisplayVersion = appEnvironnement != "prod" ? $"{appEnvironnement} - v{appVersion}" : $"v{appVersion}";
            LoggedUser = _userSettingsService.GetIngressName();
        }

        public IMvxAsyncCommand LogoutCommand => new MvxAsyncCommand(Logout);
        private async Task Logout()
        {
            await _authentificationService.LogoutAsync();

            await _navigationService.Navigate<SplashScreenViewModel>();
        }

        public string LoggedUser { get; set; }
        public string DisplayVersion { get; set; }
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

        public IMvxCommand<MenuItem> SelectedMenuItemChangedCommand => new MvxCommand<MenuItem>(SelectedMenuItemChangedExecuted);
        private void SelectedMenuItemChangedExecuted(MenuItem menuItem)
        {
            if (menuItem?.ViewModelType == null) return;

            _navigationService.Navigate(menuItem.ViewModelType);
            _selectedMenuItem = null;
        }
    }

    public class MenuItem
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public Type ViewModelType { get; set; }
    }
}