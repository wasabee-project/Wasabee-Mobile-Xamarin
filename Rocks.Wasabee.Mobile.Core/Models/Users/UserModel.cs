using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Models.Users
{
    public class UserModel : BaseModel
    {
        public string GoogleId { get; set; }
        public string IngressName { get; set; }
        public string ProfileImage { get; set; }
        public int Level { get; set; }
        public string LocationKey { get; set; }
        public string OwnTracksPw { get; set; }
        public bool VVerified { get; set; }
        public bool VBlacklisted { get; set; }
        public string Vid { get; set; }
        public string OwnTracksJson { get; set; }
        public bool RocksVerified { get; set; }
        public bool Raid { get; set; }
        public bool Risc { get; set; }
        public List<UserTeamModel> OwnedTeams { get; set; }
        public List<UserTeamModel> Teams { get; set; }
        public List<OpModel> Ops { get; set; }
        public TelegramModel Telegram { get; set; }
        public List<AssignmentModel> Assignments { get; set; }
    }

    public class TelegramModel : BaseModel
    {
        public int Id { get; set; }
        public bool Verified { get; set; }
        public string Authtoken { get; set; }
    }

    public class UserTeamModel : BaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RocksComm { get; set; }
        public string RocksKey { get; set; }
        public string State { get; set; }
        public string Owner { get; set; }
    }

    public class OpModel : BaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsOwner { get; set; }
        public string Color { get; set; }
        public string TeamId { get; set; }
    }

    public class AssignmentModel : BaseModel
    {
        public string OpId { get; set; }
        public string OperationName { get; set; }
        public string Type { get; set; }
    }
}