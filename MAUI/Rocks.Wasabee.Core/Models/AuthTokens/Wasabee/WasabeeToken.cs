using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.Models.AuthTokens.Wasabee
{
    public class WasabeeToken
    {
        public WasabeeToken(string accessToken)
        {
            AccessToken = accessToken;
        }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
    }
}
