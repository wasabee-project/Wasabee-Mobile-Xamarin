using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.Models.AuthTokens.Google
{
    public class GoogleToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("id_token")]
        public string Idtoken { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
