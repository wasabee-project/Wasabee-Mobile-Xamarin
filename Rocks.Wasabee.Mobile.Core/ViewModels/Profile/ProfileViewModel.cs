﻿using Microsoft.AppCenter.Analytics;
using MvvmCross.ViewModels;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System.Threading.Tasks;

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
                // TODO
                AgentName = "NEED API CALL";
                return;
            }

            var googleId = _userSettingsService.GetLoggedUserGoogleId();
            var userModel = await _usersDatabase.GetUserModel(googleId);
            if (userModel != null)
            {
                AgentName = userModel.IngressName;
                Level = userModel.Level;
                RocksVerified = userModel.RocksVerified;
                VVerified = userModel.VVerified;
                Picture = userModel.ProfileImage;
            }
            else
            {
                // TODO : API call
                AgentName = "ERROR";
            }

            await base.Initialize();
        }

        public string AgentName { get; set; } = string.Empty;
        public int Level { get; set; } = -1;
        public bool RocksVerified { get; set; } = false;
        public bool VVerified { get; set; } = false;
        public string Picture { get; set; } = string.Empty;
    }
}