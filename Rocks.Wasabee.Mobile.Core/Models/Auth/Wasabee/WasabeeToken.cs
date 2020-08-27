using Newtonsoft.Json;

namespace Rocks.Wasabee.Mobile.Core.Models.Auth.Wasabee
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