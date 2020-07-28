using Rocks.Wasabee.Mobile.Core.Models.Auth.Google;
using Rocks.Wasabee.Mobile.Core.Models.Auth.Wasabee;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public interface ILoginProvider
    {
        Task<GoogleOAuthResponse> DoGoogleOAuthLoginAsync();
        Task<WasabeeLoginResponse> DoWasabeeLoginAsync(GoogleOAuthResponse googleOAuthResponse);
        Task RemoveTokenFromSecureStore();
        void ClearCookie();
        Task RefreshTokenAsync();
        Task SendFirebaseTokenAsync(string token);
    }
}