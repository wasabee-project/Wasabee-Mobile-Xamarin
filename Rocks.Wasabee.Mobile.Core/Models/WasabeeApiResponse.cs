using Newtonsoft.Json;

namespace Rocks.Wasabee.Mobile.Core.Models
{
#nullable disable
    public class WasabeeApiResponse : BaseModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("updateID")]
        public string UpdateId { get; set; }
    }

    public class WasabeeJwtRefreshApiResponse : WasabeeApiResponse
    {
        [JsonProperty("jwk")]
        public string Token { get; set; }
    }
#nullable enable

    public static class WasabeeApiResponseExtensions
    {
        public static bool IsSuccess(this WasabeeApiResponse response)
        {
            return string.IsNullOrWhiteSpace(response.Error) && response.Status.Equals("ok");
        }

        public static bool HasUpdateId(this WasabeeApiResponse response)
        {
            return !string.IsNullOrWhiteSpace(response.UpdateId);
        }
    }
}