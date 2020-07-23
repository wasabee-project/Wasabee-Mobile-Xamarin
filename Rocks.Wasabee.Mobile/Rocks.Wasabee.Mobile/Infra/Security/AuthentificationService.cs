﻿using System.Threading.Tasks;
using Rocks.Wasabee.Mobile.Core.Models.Wasabee;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public class AuthentificationService : IAuthentificationService
    {
        private readonly ILoginProvider _loginProvider;

        private bool _isCacheCleared;

        public AuthentificationService(ILoginProvider loginProvider)
        {
            _loginProvider = loginProvider;
        }

        public async Task<GoogleOAuthResponse> GoogleLoginAsync()
        {
            if (!_isCacheCleared)
            {
                await ClearUserTokenAndCookie(_loginProvider);
                _isCacheCleared = true;
            }

            return await _loginProvider.DoGoogleOAuthLoginAsync();
        }

        public async Task<WasabeeLoginResponse> WasabeeLoginAsync(GoogleOAuthResponse googleOAuthResponse)
        {
            return await _loginProvider.DoWasabeeLoginAsync(googleOAuthResponse);
        }

        public async Task LogoutAsync()
        {
            await ClearUserTokenAndCookie(_loginProvider);
            _isCacheCleared = true;
        }

        public async Task RefreshTokenAsync()
        {
            await _loginProvider.RefreshTokenAsync();
        }

        private async Task ClearUserTokenAndCookie(ILoginProvider loginProvider)
        {
            await loginProvider.RemoveTokenFromSecureStore();
            loginProvider.ClearCookie();
        }
    }
}