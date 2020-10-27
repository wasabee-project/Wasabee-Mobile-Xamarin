using Newtonsoft.Json;
using System;

namespace Rocks.Wasabee.Mobile.Core.Models.AuthTokens.Google
{
#nullable disable
    public class GoogleToken
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

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
#nullable enable
}