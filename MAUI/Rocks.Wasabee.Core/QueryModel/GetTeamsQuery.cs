using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.QueryModel
{
    public class GetTeamsQuery
    {
        public GetTeamsQuery(List<string> teamIds)
        {
            TeamIds = teamIds;
        }

        [JsonPropertyName("teamids")]
        public List<string> TeamIds { get; set; }
    }
}
