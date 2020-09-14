using Newtonsoft.Json;

namespace Rocks.Wasabee.Mobile.Core.Models.AuthTokens.Wasabee
{
    public class WasabeeToken
    {
        public WasabeeToken(string accessToken)
        {
            AccessToken = accessToken;
        }

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
    }
}