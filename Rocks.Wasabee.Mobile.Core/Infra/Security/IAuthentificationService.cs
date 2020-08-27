using Rocks.Wasabee.Mobile.Core.Models.Auth.Google;
using Rocks.Wasabee.Mobile.Core.Models.Auth.Wasabee;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public interface IAuthentificationService
    {
        Task<GoogleToken> GoogleLoginAsync();
        Task<WasabeeLoginResponse> WasabeeLoginAsync(GoogleToken googleToken);
        Task LogoutAsync();
        Task RefreshTokenAsync();
    }
}