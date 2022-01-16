using Newtonsoft.Json;

namespace Rocks.Wasabee.Mobile.Core.Infra.Firebase.Payloads
{
    public class GenericMessagePayload
    {
        [JsonProperty("sender")]
        public string Sender { get; set; } = string.Empty;

        [JsonProperty("msg")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("srv")]
        public string Server { get; set; } = string.Empty;

        [JsonProperty("opID")]
        public string OperationId { get; set; } = string.Empty;
    }
}