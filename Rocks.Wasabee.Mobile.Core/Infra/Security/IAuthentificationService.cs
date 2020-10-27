using Rocks.Wasabee.Mobile.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using System.Threading.Tasks;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public interface IAuthentificationService
    {
        Task<GoogleToken?> GoogleLoginAsync();
        Task<UserModel?> WasabeeLoginAsync(GoogleToken googleToken);
        Task LogoutAsync();
        Task<GoogleToken?> RefreshTokenAsync(string refreshToken);
    }
}