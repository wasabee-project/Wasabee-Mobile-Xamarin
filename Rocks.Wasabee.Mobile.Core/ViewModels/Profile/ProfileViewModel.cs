using Microsoft.AppCenter.Analytics;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Rocks.Wasabee.Mobile.Core.ViewModels.Profile
{
    public class ProfileViewModelNavigationParameter
    {
        public string UserGoogleId { get; }

        public ProfileViewModelNavigationParameter(string userGoogleId)
        {
            UserGoogleId = userGoogleId;
        }
    }

    public class ProfileViewModel : BaseViewModel, IMvxViewModel<ProfileViewModelNavigationParameter>
    {
        private readonly UsersDatabase _usersDatabase;
        private readonly IUserSettingsService _userSettingsService;

        private ProfileViewModelNavigationParameter? _parameter;

        public ProfileViewModel(UsersDatabase usersDatabase, IUserSettingsService userSettingsService)
        {
            _usersDatabase = usersDatabase;
            _userSettingsService = userSettingsService;
        }

        public void Prepare(ProfileViewModelNavigationParameter parameter)
        {
            _parameter = parameter;
        }

        public override async Task Initialize()
        {
            Analytics.TrackEvent(GetType().Name);
            LoggingService.Trace("Navigated to ProfileViewModel");

            if (_parameter != null)
            {
                IsSelfProfile = false;

                // TODO
                User = new UserModel { IngressName = "NEED API CALL" };
                return;
            }

            var googleId = _userSettingsService.GetLoggedUserGoogleId();
            var userModel = await _usersDatabase.GetUserModel(googleId);
            if (userModel != null)
            {
                User = userModel;
                QrCodeValue = userModel.GoogleId;
            }
            else
            {
                // TODO : API call
                User = new UserModel { IngressName = "ERROR" };
            }

            await base.Initialize();
        }

        #region Properties

        public UserModel? User { get; set; }
        public bool IsSelfProfile { get; set; } = true;
        public bool IsQrCodeVisible { get; set; }
        public string QrCodeValue { get; set; }

        #endregion

        #region Commands

        public IMvxAsyncCommand OpenRocksProfileCommand => new MvxAsyncCommand(OpenRocksProfileExecuted);
        private async Task OpenRocksProfileExecuted()
        {
            if (IsBusy || User == null)
                return;

            if (string.IsNullOrWhiteSpace(User.GoogleId))
                return;

            LoggingService.Trace("Executing ProfileViewModel.OpenRocksProfileCommand");

            try
            {
                IsBusy = true;

                var link = $"https://enlightened.rocks/u/{User.GoogleId}";
                await Launcher.OpenAsync(new Uri(link));
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing ProfileViewModel.OpenRocksProfileCommand");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IMvxAsyncCommand OpenVProfileCommand => new MvxAsyncCommand(OpenVProfileExecuted);
        private async Task OpenVProfileExecuted()
        {
            if (IsBusy || User == null)
                return;

            if (string.IsNullOrWhiteSpace(User.GoogleId))
                return;

            LoggingService.Trace("Executing ProfileViewModel.OpenVProfileCommand");

            try
            {
                IsBusy = true;

                var link = $"https://v.enl.one/profile/{User.EnlId}";
                await Launcher.OpenAsync(new Uri(link));
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Executing ProfileViewModel.OpenVProfileCommand");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IMvxCommand ShowQrCodeCommand => new MvxCommand(ShowQrCodeExecuted);
        private void ShowQrCodeExecuted()
        {
            IsQrCodeVisible = !IsQrCodeVisible;
        }

        #endregion
    }
}