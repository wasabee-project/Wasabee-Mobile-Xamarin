using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using Rocks.Wasabee.Mobile.Core.Settings.User;
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

        public async Task LogoutAsync()
        {
            _loggingService.Trace("Executing AuthentificationService.LogoutAsync");

            _preferences.Remove(UserSettingsKeys.RememberServerChoice);
            _preferences.Remove(UserSettingsKeys.SavedServerChoice);

            await ClearUserTokenAndCookie(_loginProvider);
        }

        public async Task<GoogleToken?> RefreshTokenAsync(string refreshToken)
        {
            _loggingService.Trace("Executing AuthentificationService.RefreshTokenAsync");

            return await _loginProvider.RefreshTokenAsync(refreshToken);
        }

        private async Task ClearUserTokenAndCookie(ILoginProvider loginProvider)
        {
            _loggingService.Trace("Executing AuthentificationService.ClearUserTokenAndCookie");

            await loginProvider.RemoveTokenFromSecureStore();
            loginProvider.ClearCookie();
        }
    }
}