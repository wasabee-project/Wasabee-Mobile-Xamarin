using Rocks.Wasabee.Core.Models.AuthTokens.Google;
using Rocks.Wasabee.Core.Models.Users;

namespace Rocks.Wasabee.Core.Infra.Security
{
    public interface ILoginProvider
    {
        Task<GoogleToken?> DoGoogleOAuthLoginAsync();
        Task<UserModel?> DoWasabeeLoginAsync(GoogleToken googleToken);
        Task<UserModel?> DoWasabeeOneTimeTokenLoginAsync(string oneTimeToken);
        Task<GoogleToken?> RefreshGoogleTokenAsync(string refreshToken);
        Task RemoveTokensFromSecureStore();
    }
}
