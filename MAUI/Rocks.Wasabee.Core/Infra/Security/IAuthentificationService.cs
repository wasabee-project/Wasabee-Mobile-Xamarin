using Rocks.Wasabee.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Core.Models.Users;

namespace Rocks.Wasabee.Core.Infra.Security
{
    public interface IAuthentificationService
    {
        Task<GoogleToken?> GoogleLoginAsync();
        Task<UserModel?> WasabeeLoginAsync(GoogleToken googleToken);
        Task<UserModel?> WasabeeOneTimeTokenLoginAsync(string oneTimeToken);
        Task LogoutAsync();
        Task<GoogleToken?> RefreshGoogleTokenAsync(string refreshToken);
    }
}
