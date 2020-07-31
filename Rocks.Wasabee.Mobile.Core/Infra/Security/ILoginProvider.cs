using Rocks.Wasabee.Mobile.Core.Models.Auth.Google;
using Rocks.Wasabee.Mobile.Core.Models.Auth.Wasabee;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public interface ILoginProvider
    {
        Task<GoogleToken> DoGoogleOAuthLoginAsync();
        Task<WasabeeLoginResponse> DoWasabeeLoginAsync(GoogleToken googleToken);
        Task RemoveTokenFromSecureStore();
        void ClearCookie();
        Task RefreshTokenAsync();
        Task SendFirebaseTokenAsync(string token);
    }
}