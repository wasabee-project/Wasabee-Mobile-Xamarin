using Rocks.Wasabee.Core.Models.Agent;
using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.Models.Users
{
    public class UserModel : AgentModel
    {
        [JsonPropertyName("GoogleID")]
        public string GoogleId { get; set; }

        [JsonPropertyName("lockey")]
        public string OneTimeToken { get; set; }

        [JsonPropertyName("Teams")]
        public List<UserTeamModel> Teams { get; set; }

        [JsonPropertyName("Ops")]
        public List<OpModel> Ops { get; set; }

        [JsonPropertyName("Telegram")]
        public TelegramModel Telegram { get; set; }

        [JsonPropertyName("querytoken")]
        public string QueryToken { get; set; }

        [JsonPropertyName("jwt")]
        public string Jwt { get; set; }
    }

    public class TelegramModel : BaseModel
    {
        [JsonPropertyName("ID")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("Verified")]
        public bool Verified { get; set; }

        [JsonPropertyName("Authtoken")]
        public string AuthToken { get; set; }
    }

    public class UserTeamModel : BaseModel
    {
        [JsonPropertyName("ID")]
        public string Id { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("RocksComm")]
        public string RocksComm { get; set; }

        [JsonPropertyName("RocksKey")]
        public string RocksKey { get; set; }

        [JsonPropertyName("JoinLinkToken")]
        public string JoinLinkToken { get; set; }

        [JsonPropertyName("State")]
        public string State { get; set; }

        [JsonPropertyName("ShareWD")]
        public string ShareWD { get; set; }

        [JsonPropertyName("LoadWD")]
        public string LoadWD { get; set; }

        [JsonPropertyName("Owner")]
        public string Owner { get; set; }
    }

    public class OpModel : BaseModel
    {
        [JsonPropertyName("ID")]
        public string Id { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("IsOwner")]
        public bool IsOwner { get; set; }

        [JsonPropertyName("Color")]
        public string Color { get; set; }

        [JsonPropertyName("TeamID")]
        public string TeamId { get; set; }

        [JsonPropertyName("Modified")]
        public string Modified { get; set; }

        [JsonPropertyName("LastEditID")]
        public string LastEditId { get; set; }
    }
}
