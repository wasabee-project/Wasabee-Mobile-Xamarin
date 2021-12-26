using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Models.Agent;
using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Models.Users
{
#nullable disable
    public class UserModel : AgentModel
    {
        [JsonProperty("GoogleID")]
        public string GoogleId { get; set; }
        
        [JsonProperty("lockey")]
        public string LocationKey { get; set; }

        [JsonProperty("enlid")]
        public string EnlId { get; set; }

        [JsonProperty("pic")]
        public string ProfileImage { get; set; }

        [JsonProperty("Teams")]
        public List<UserTeamModel> Teams { get; set; }

        [JsonProperty("Ops")]
        public List<OpModel> Ops { get; set; }

        [JsonProperty("Telegram")]
        public TelegramModel Telegram { get; set; }

        [JsonProperty("querytoken")]
        public string QueryToken { get; set; }

        [JsonProperty("jwt")]
        public string Jwt { get; set; }
    }

    public class TelegramModel : BaseModel
    {
        [JsonProperty("ID")]
        public int Id { get; set; }

        [JsonProperty("Verified")]
        public bool Verified { get; set; }
    }

    public class UserTeamModel : BaseModel
    {
        [JsonProperty("ID")]
        public string Id { get; set; }
        
        [JsonProperty("Name")]
        public string Name { get; set; }
        
        [JsonProperty("RocksComm")]
        public string RocksComm { get; set; }
        
        [JsonProperty("RocksKey")]
        public string RocksKey { get; set; }
        
        [JsonProperty("JoinLinkToken")]
        public string JoinLinkToken { get; set; }
        
        [JsonProperty("State")]
        public string State { get; set; }
        
        [JsonProperty("ShareWD")]
        public string ShareWD { get; set; }
        
        [JsonProperty("LoadWD")]
        public string LoadWD { get; set; }
        
        [JsonProperty("Owner")]
        public string Owner { get; set; }
    }

    public class OpModel : BaseModel
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("IsOwner")]
        public bool IsOwner { get; set; }

        [JsonProperty("Color")]
        public string Color { get; set; }

        [JsonProperty("TeamID")]
        public string TeamId { get; set; }

        [JsonProperty("Modified")]
        public string Modified { get; set; }

        [JsonProperty("LastEditID")]
        public string LastEditId { get; set; }
    }
#nullable enable
}