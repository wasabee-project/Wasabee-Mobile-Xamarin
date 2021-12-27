using MvvmCross;
using Rocks.Wasabee.Mobile.Core.Infra.Databases;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using Rocks.Wasabee.Mobile.Core.Settings.User;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public class AuthentificationService : IAuthentificationService
    {
        private readonly ILoginProvider _loginProvider;
        private readonly IPreferences _preferences;
        private readonly ILoggingService _loggingService;

        public AuthentificationService(ILoginProvider loginProvider, IPreferences preferences, ILoggingService loggingService)
        {
            _loginProvider = loginProvider;
            _preferences = preferences;
            _loggingService = loggingService;
        }

        public async Task<GoogleToken?> GoogleLoginAsync()
        {
            _loggingService.Trace("Executing AuthentificationService.GoogleLoginAsync");

            return await _loginProvider.DoGoogleOAuthLoginAsync();
        }

        public async Task<UserModel?> WasabeeLoginAsync(GoogleToken googleToken)
        {
            _loggingService.Trace("Executing AuthentificationService.WasabeeLoginAsync");

            return await _loginProvider.DoWasabeeLoginAsync(googleToken);
        }

        public async Task<UserModel?> WasabeeOneTimeTokenLoginAsync(string oneTimeToken)
        {
            _loggingService.Trace("Executing AuthentificationService.WasabeeOneTimeTokenLoginAsync");

            return await _loginProvider.DoWasabeeOneTimeTokenLoginAsync(oneTimeToken);
        }

        public async Task LogoutAsync()
        {
            _loggingService.Trace("Executing AuthentificationService.LogoutAsync");

            _preferences.Remove(UserSettingsKeys.RememberServerChoice);
            _preferences.Remove(UserSettingsKeys.SavedServerChoice);
            _preferences.Remove(UserSettingsKeys.LastLoginMethod);

            await ClearTokens();
            await ClearDatabases();
        }

        public async Task<GoogleToken?> RefreshGoogleTokenAsync(string refreshToken)
        {
            _loggingService.Trace("Executing AuthentificationService.RefreshGoogleTokenAsync");

            return await _loginProvider.RefreshGoogleTokenAsync(refreshToken);
        }

        private async Task ClearTokens()
        {
            _loggingService.Trace("Executing AuthentificationService.ClearUserTokenAndCookie");

            await _loginProvider.RemoveTokensFromSecureStore();
        }

        private async Task ClearDatabases()
        {
            _loggingService.Trace("Executing AuthentificationService.ClearDatabases");

            try
            {
                await Mvx.IoCProvider.Resolve<UsersDatabase>().DeleteAllData();
                await Mvx.IoCProvider.Resolve<TeamsDatabase>().DeleteAllData();
                await Mvx.IoCProvider.Resolve<TeamAgentsDatabase>().DeleteAllData();
                await Mvx.IoCProvider.Resolve<OperationsDatabase>().DeleteAllData();
                await Mvx.IoCProvider.Resolve<LinksDatabase>().DeleteAllData();
                await Mvx.IoCProvider.Resolve<MarkersDatabase>().DeleteAllData();
            }
            catch (Exception e)
            {
                _loggingService.Error(e, "Error Executing AuthentificationService.ClearDatabases");
            }
        }
    }
}