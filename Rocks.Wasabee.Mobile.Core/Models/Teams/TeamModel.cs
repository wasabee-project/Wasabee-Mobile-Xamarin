using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Models.Teams
{
#nullable disable
    public class TeamModel : BaseModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("agents")]
        public List<TeamAgentModel> Agents { get; set; }

        [JsonIgnore]
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }

    public class TeamAgentModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("enlid")]
        public string Enlid { get; set; }

        [JsonProperty("pic")]
        public string Pic { get; set; }

        [JsonProperty("rocks")]
        public bool RocksVerified { get; set; }

        [JsonProperty("Vverified")]
        public bool VVerified { get; set; }

        [JsonProperty("blacklisted")]
        public bool Blacklisted { get; set; }

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

        [JsonProperty("ShareWD")]
        public bool ShareWD { get; set; }

        [JsonProperty("LoadWD")]
        public bool LoadWD { get; set; }

        [JsonIgnore]
        public DateTime LastUpdatedAt { get; set; }
    }
#nullable enable
}
