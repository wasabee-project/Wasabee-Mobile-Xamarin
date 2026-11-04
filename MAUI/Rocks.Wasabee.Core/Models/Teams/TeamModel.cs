using Rocks.Wasabee.Core.Models.Agent;
using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.Models.Teams
{
    public class TeamModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("agents")]
        public List<AgentModel> Agents { get; set; }

        [JsonIgnore]
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }
}
