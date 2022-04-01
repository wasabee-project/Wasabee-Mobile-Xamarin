using Newtonsoft.Json;

namespace Rocks.Wasabee.Mobile.Core.Infra.Firebase.Payloads
{
    public class TargetPayload
    {
        [JsonProperty("ID")]
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        [JsonProperty("Lat")]
        public string Latitude { get; set; } = string.Empty;

        [JsonProperty("Lng")]
        public string Longitude { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Sender { get; set; } = string.Empty;
    }
}