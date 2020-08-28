using Rocks.Wasabee.Mobile.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public interface ILoginProvider
    {
        Task<GoogleToken> DoGoogleOAuthLoginAsync();
        Task<UserModel> DoWasabeeLoginAsync(GoogleToken googleToken);
        Task RemoveTokenFromSecureStore();
        void ClearCookie();
        Task RefreshTokenAsync();
        Task SendFirebaseTokenAsync(string token);
    }
}