using Rocks.Wasabee.Core.Infra.Security;
using Rocks.Wasabee.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Core.Models.Users;

namespace Rocsk.Wasabee.Maui.Core.Services
{
    public class AuthentificationService : IAuthentificationService
    {
        private ILoginProvider _loginProvider;

        public AuthentificationService(ILoginProvider loginProvider)
        {
            _loginProvider = loginProvider;
        }

        public async Task<GoogleToken?> GoogleLoginAsync()
        {
            return await _loginProvider.DoGoogleOAuthLoginAsync();
        }

        public async Task<UserModel?> WasabeeLoginAsync(GoogleToken googleToken)
        {
            return await _loginProvider.DoWasabeeLoginAsync(googleToken);
        }

        public async Task<UserModel?> WasabeeOneTimeTokenLoginAsync(string oneTimeToken)
        {
            return await _loginProvider.DoWasabeeOneTimeTokenLoginAsync(oneTimeToken);
        }

        public async Task LogoutAsync()
        {
            //_preferences.Remove(UserSettingsKeys.RememberServerChoice);
            //_preferences.Remove(UserSettingsKeys.SavedServerChoice);
            //_preferences.Remove(UserSettingsKeys.LastLoginMethod);

            await ClearTokens();
            await ClearDatabases();
        }

        public async Task<GoogleToken?> RefreshGoogleTokenAsync(string refreshToken)
        {

            return await _loginProvider.RefreshGoogleTokenAsync(refreshToken);
        }

        private async Task ClearTokens()
        {
            await _loginProvider.RemoveTokensFromSecureStore();
        }

        private async Task ClearDatabases()
        {
            try
            {
                //await Mvx.IoCProvider.Resolve<UsersDatabase>().DeleteAllData();
                //await Mvx.IoCProvider.Resolve<TeamsDatabase>().DeleteAllData();
                //await Mvx.IoCProvider.Resolve<AgentsDatabase>().DeleteAllData();
                //await Mvx.IoCProvider.Resolve<OperationsDatabase>().DeleteAllData();
                //await Mvx.IoCProvider.Resolve<LinksDatabase>().DeleteAllData();
                //await Mvx.IoCProvider.Resolve<MarkersDatabase>().DeleteAllData();
            }
            catch (Exception e)
            {
            }
        }
    }
}
