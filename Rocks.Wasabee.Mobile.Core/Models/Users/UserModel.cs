using Newtonsoft.Json;
using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Models.Users
{
#nullable disable
    public class UserModel : BaseModel
    {
        [JsonProperty("GoogleID")]
        public string GoogleId { get; set; }

        [JsonProperty("enlid")]
        public string EnlId { get; set; }

        [JsonProperty("name")]
        public string IngressName { get; set; }

        [JsonProperty("vname")]
        public string VName { get; set; }

        [JsonProperty("rocksname")]
        public string RocksName { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("lockey")]
        public string LocationKey { get; set; }

        [JsonProperty("rocks")]
        public bool RocksVerified { get; set; }

        [JsonProperty("Vverified")]
        public bool VVerified { get; set; }

        [JsonProperty("blacklisted")]
        public bool Blacklisted { get; set; }

        [JsonProperty("pic")]
        public string ProfileImage { get; set; }

        [JsonProperty("RAID")]
        public bool Raid { get; set; }

        [JsonProperty("RISC")]
        public bool Risc { get; set; }

        [JsonProperty("intelfaction")]
        public string IntelFaction { get; set; }

        [JsonProperty("Telegram")]
        public TelegramModel Telegram { get; set; }
        
        [JsonProperty("Teams")]
        public List<UserTeamModel> Teams { get; set; }

        [JsonProperty("Ops")]
        public List<OpModel> Ops { get; set; }

        [JsonProperty("Assignments")]
        public List<AssignmentModel> Assignments { get; set; }

        [JsonProperty("jwt")]
        public string Jwt { get; set; }
    }

    public class TelegramModel : BaseModel
    {
        [JsonProperty("ID")]
        public int Id { get; set; }

        [JsonProperty("Verified")]
        public bool Verified { get; set; }

        [JsonProperty("Authtoken")]
        public string Authtoken { get; set; }
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
        public string LastEditID { get; set; }
    }

    public class AssignmentModel : BaseModel
    {
        [JsonProperty("OpID")]
        public string OpId { get; set; }
        
        [JsonProperty("OperationName")]
        public string OperationName { get; set; }
    }
#nullable enable
}