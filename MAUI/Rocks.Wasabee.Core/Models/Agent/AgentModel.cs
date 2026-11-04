using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.Models.Agent
{
    public class AgentModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("rocksname")]
        public string RocksName { get; set; }

        [JsonPropertyName("intelname")]
        public string IntelName { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("pic")]
        public string ProfileImage { get; set; }

        [JsonPropertyName("blacklisted")]
        public bool Blacklisted { get; set; }

        [JsonPropertyName("rocks")]
        public bool RocksVerified { get; set; }

        [JsonPropertyName("smurf")]
        public bool Smurf { get; set; }

        [JsonPropertyName("intelfaction")]
        public string IntelFaction { get; set; }

        [JsonPropertyName("squad")]
        public string Squad { get; set; }

        [JsonPropertyName("state")]
        public bool State { get; set; }

        [JsonPropertyName("lat")]
        public float Lat { get; set; }

        [JsonPropertyName("lng")]
        public float Lng { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("shareWD")]
        public bool ShareWD { get; set; }

        [JsonPropertyName("loadWD")]
        public bool LoadWD { get; set; }

        [JsonIgnore]
        public DateTime LastUpdatedAt { get; set; }
    }
}
