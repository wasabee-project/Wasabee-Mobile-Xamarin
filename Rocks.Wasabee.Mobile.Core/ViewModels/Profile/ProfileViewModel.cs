using Microsoft.AppCenter.Analytics;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using Rocks.Wasabee.Mobile.Core.Services;
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
        private readonly WasabeeApiV1Service _wasabeeApiV1Service;
        private readonly IUserSettingsService _userSettingsService;

        private ProfileViewModelNavigationParameter? _parameter;

        public ProfileViewModel(UsersDatabase usersDatabase, WasabeeApiV1Service wasabeeApiV1Service,
            IUserSettingsService userSettingsService)
        {
            _usersDatabase = usersDatabase;
            _wasabeeApiV1Service = wasabeeApiV1Service;
            _userSettingsService = userSettingsService;
        }

        public void Prepare(ProfileViewModelNavigationParameter parameter)
        {
            _parameter = parameter;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            Analytics.TrackEvent(GetType().Name);
            LoggingService.Trace("Navigated to ProfileViewModel");

            if (_parameter != null)
            {
                base.HasHistory = true;

                IsSelfProfile = false;
                IsBusy = true;
            
                QrCodeValue = _parameter.UserGoogleId;
            
                await LoadAgentProfileCommand.ExecuteAsync(_parameter.UserGoogleId);
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
                QrCodeValue = googleId;
                await LoadAgentProfileCommand.ExecuteAsync(googleId);
            }
        }

        #region Properties

        public UserModel? User { get; set; }
        public bool IsSelfProfile { get; set; } = true;
        public bool IsQrCodeVisible { get; set; }
        public string QrCodeValue { get; set; } = string.Empty;

        #endregion

        #region Commands

        public IMvxAsyncCommand<string> LoadAgentProfileCommand => new MvxAsyncCommand<string>(async agentId => await LoadAgentProfileExecuted(agentId));
        private async Task LoadAgentProfileExecuted(string agentId)
        {
            LoggingService.Trace("Executing ProfileViewModel.LoadAgentProfileCommand");

            if (agentId.IsNullOrEmpty())
            {
                IsBusy = false;
                return;
            }

            var agent = await _wasabeeApiV1Service.Agents_GetAgent(agentId);
            if (agent == null)
            {
                IsBusy = false;
                return;
            }

            User = new UserModel()
            {
                IngressName = agent.Name,
                ProfileImage = agent.Pic,
                Level = agent.Level,
                GoogleId = agent.Id,
                EnlId = agent.Enlid,
                RocksVerified = agent.RocksVerified,
                VVerified = agent.VVerified,
                Blacklisted = agent.Blacklisted
            };

            IsBusy = false;
        }

        public IMvxAsyncCommand OpenRocksProfileCommand => new MvxAsyncCommand(OpenRocksProfileExecuted);
        private async Task OpenRocksProfileExecuted()
        {
            if (IsBusy || User == null)
                return;

            if (User.GoogleId.IsNullOrEmpty())
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

            if (User.GoogleId.IsNullOrEmpty())
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