using System.Threading.Tasks;
using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Models.Wasabee;

namespace Rocks.Wasabee.Mobile.Core.Infra.Security
{
    public interface ILoginProvider
    {
        Task<GoogleOAuthResponse> DoGoogleOAuthLoginAsync();
        Task<WasabeeLoginResponse> DoWasabeeLoginAsync(GoogleOAuthResponse googleOAuthResponse);
        Task RemoveTokenFromSecureStore();
        void ClearCookie();
        Task RefreshTokenAsync();
    }

    public class GoogleOAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("id_token")]
        public string Idtoken { get; set; }
    }
}