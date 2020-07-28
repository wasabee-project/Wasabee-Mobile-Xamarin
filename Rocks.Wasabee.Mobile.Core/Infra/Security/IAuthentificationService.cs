using Rocks.Wasabee.Mobile.Core.Models.Auth.Google;
using Rocks.Wasabee.Mobile.Core.Models.Auth.Wasabee;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public interface IAuthentificationService
    {
        Task<GoogleOAuthResponse> GoogleLoginAsync();
        Task<WasabeeLoginResponse> WasabeeLoginAsync(GoogleOAuthResponse googleOAuthResponse);
        Task LogoutAsync();
        Task RefreshTokenAsync();
    }
}