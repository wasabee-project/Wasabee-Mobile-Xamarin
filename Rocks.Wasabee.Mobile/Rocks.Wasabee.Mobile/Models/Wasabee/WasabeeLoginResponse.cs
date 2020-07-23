namespace Rocks.Wasabee.Mobile.Core.Models.Wasabee
{
    public class WasabeeLoginResponse
    {
        public string GoogleId { get; set; }
        public string IngressName { get; set; }
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
        public OwnedTeam[] OwnedTeams { get; set; }
        public Team[] Teams { get; set; }
        public Op[] Ops { get; set; }
        public OwnedOp[] OwnedOps { get; set; }
        public Telegram Telegram { get; set; }
        public Assignment[] Assignments { get; set; }
    }

    public class Telegram
    {
        public string UserName { get; set; }
        public int Id { get; set; }
        public bool Verified { get; set; }
        public string Authtoken { get; set; }
    }

    public class OwnedTeam
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RocksComm { get; set; }
        public string RocksKey { get; set; }
    }

    public class Team
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string RocksComm { get; set; }
    }

    public class Op
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string TeamName { get; set; }
        public string TeamId { get; set; }
    }

    public class OwnedOp
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string TeamName { get; set; }
        public string TeamId { get; set; }
    }

    public class Assignment
    {
        public string OpId { get; set; }
        public string OperationName { get; set; }
        public string Type { get; set; }
    }
}