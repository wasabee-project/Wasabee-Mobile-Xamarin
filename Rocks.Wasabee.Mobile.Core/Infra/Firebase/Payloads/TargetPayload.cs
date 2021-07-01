using Newtonsoft.Json;

namespace Rocks.Wasabee.Mobile.Core.Infra.Firebase.Payloads
{
    public class TargetPayload
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonProperty("Lat")]
        public string Latitude { get; set; }

        [JsonProperty("Lon")]
        public string Longitude { get; set; }

        public string Type { get; set; }

        public string Sender { get; set; }
    }
}