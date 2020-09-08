using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public class AuthentificationService : IAuthentificationService
    {
        private readonly ILoginProvider _loginProvider;
        private readonly ILoggingService _loggingService;

        private bool _isCacheCleared;

        public AuthentificationService(ILoginProvider loginProvider, ILoggingService loggingService)
        {
            _loginProvider = loginProvider;
            _loggingService = loggingService;
        }

        public async Task<GoogleToken> GoogleLoginAsync()
        {
            _loggingService.Trace("Executing AuthentificationService.GoogleLoginAsync");

            return await _loginProvider.DoGoogleOAuthLoginAsync();
        }

        public async Task<UserModel> WasabeeLoginAsync(GoogleToken googleToken)
        {
            _loggingService.Trace("Executing AuthentificationService.WasabeeLoginAsync");

            return await _loginProvider.DoWasabeeLoginAsync(googleToken);
        }

        public async Task LogoutAsync()
        {
            _loggingService.Trace("Executing AuthentificationService.LogoutAsync");

            await ClearUserTokenAndCookie(_loginProvider);
            _isCacheCleared = true;
        }

        public async Task RefreshTokenAsync()
        {
            _loggingService.Trace("Executing AuthentificationService.RefreshTokenAsync");

            await _loginProvider.RefreshTokenAsync();
        }

        private async Task ClearUserTokenAndCookie(ILoginProvider loginProvider)
        {
            _loggingService.Trace("Executing AuthentificationService.ClearUserTokenAndCookie");

            await loginProvider.RemoveTokenFromSecureStore();
            loginProvider.ClearCookie();
        }
    }
}