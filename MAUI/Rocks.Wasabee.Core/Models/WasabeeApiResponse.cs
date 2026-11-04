using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.Models
{
    public class WasabeeApiResponse : BaseModel
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        public bool IsSuccess()
        {
            return string.IsNullOrWhiteSpace(Error) && Status.Equals("ok");
        }
    }
    public class WasabeeOpUpdateApiResponse : WasabeeApiResponse
    {
        [JsonPropertyName("updateID")]
        public string UpdateId { get; set; }
    }

    public class WasabeeJwtApiResponse : WasabeeApiResponse
    {
        [JsonPropertyName("jwt")]
        public string Token { get; set; }
    }
}
