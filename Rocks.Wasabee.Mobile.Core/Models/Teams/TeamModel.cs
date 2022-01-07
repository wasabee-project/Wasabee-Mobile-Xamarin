using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Models.Agent;
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
        public List<AgentModel> Agents { get; set; }

        [JsonIgnore]
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }
#nullable enable
}
