using System.Threading.Tasks;
using Rocks.Wasabee.Mobile.Core.Models.Wasabee;

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