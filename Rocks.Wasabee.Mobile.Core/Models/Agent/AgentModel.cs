using Newtonsoft.Json;
using System;

namespace Rocks.Wasabee.Mobile.Core.Models.Agent
{
#nullable disable
    public class AgentModel : BaseModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("vname")]
        public string VName { get; set; }

        [JsonProperty("rocksname")]
        public string RocksName { get; set; }

        [JsonProperty("intelname")]
        public string IntelName { get; set; }
        
        [JsonProperty("communityname")]
        public string CommunityName { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("enlid")]
        public string Enlid { get; set; }

        [JsonProperty("pic")]
        public string Pic { get; set; }

        [JsonProperty("Vverified")]
        public bool VVerified { get; set; }

        [JsonProperty("blacklisted")]
        public bool Blacklisted { get; set; }

        [JsonProperty("rocks")]
        public bool RocksVerified { get; set; }

        [JsonProperty("smurf")]
        public bool Smurf { get; set; }

        [JsonProperty("intelfaction")]
        public string IntelFaction { get; set; }

        [JsonProperty("squad")]
        public string Squad { get; set; }

        [JsonProperty("state")]
        public bool State { get; set; }

        [JsonProperty("lat")]
        public float Lat { get; set; }

        [JsonProperty("lng")]
        public float Lng { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("shareWD")]
        public bool ShareWD { get; set; }

        [JsonProperty("loadWD")]
        public bool LoadWD { get; set; }

        [JsonIgnore]
        public DateTime LastUpdatedAt { get; set; }
    }
#nullable enable
}