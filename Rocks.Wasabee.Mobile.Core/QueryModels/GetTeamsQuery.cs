using Newtonsoft.Json;
using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.QueryModels
{
    public class GetTeamsQuery
    {
        public GetTeamsQuery(List<string> teamIds)
        {
            TeamIds = teamIds;
        }

        [JsonProperty("teamids")]
        public List<string> TeamIds { get; set; }
    }
}