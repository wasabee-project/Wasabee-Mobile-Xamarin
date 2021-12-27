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

        public bool IsSuccess()
        {
            return string.IsNullOrWhiteSpace(Error) && Status.Equals("ok");
        }
    }
    public class WasabeeOpUpdateApiResponse : WasabeeApiResponse
    {
        [JsonProperty("updateID")]
        public string UpdateId { get; set; }
    }

    public class WasabeeJwtApiResponse : WasabeeApiResponse
    {
        [JsonProperty("jwt")]
        public string Token { get; set; }
    }
#nullable enable
}