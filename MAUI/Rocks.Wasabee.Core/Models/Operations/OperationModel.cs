using Rocks.Wasabee.Core.Models.Converters;
using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.Models.Operations
{
    public class OperationModel : BaseModel
    {
        [JsonPropertyName("ID")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("creator")]
        public string Creator { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("opportals")]
        public List<PortalModel>? Portals { get; set; }

        [JsonPropertyName("anchors")]
        public List<string>? Anchors { get; set; }

        [JsonPropertyName("links")]
        public List<LinkModel>? Links { get; set; }

        [JsonPropertyName("blockers")]
        public List<BlockerModel>? Blockers { get; set; }

        [JsonPropertyName("markers")]
        public List<MarkerModel>? Markers { get; set; }

        [JsonPropertyName("teamlist")]
        public List<TeamModel>? TeamList { get; set; }

        [JsonPropertyName("keysonhand")]
        public List<KeysOnHandModel>? KeysOnHand { get; set; }

        [JsonPropertyName("zones")]
        public List<ZoneModel>? Zones { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("modified")]
        public string Modified { get; set; }

        [JsonPropertyName("lasteditid")]
        public string LastEditId { get; set; }

        [JsonIgnore]
        public bool IsHiddenLocally { get; set; } = false;

        [JsonIgnore]
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }

    public class PortalModel : BaseModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("lat")]
        public string Lat { get; set; }

        [JsonPropertyName("lng")]
        public string Lng { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("hardness")]
        public string Hardness { get; set; }
    }

    public class LinkModel : TaskModel
    {
        [JsonPropertyName("fromPortalId")]
        public string FromPortalId { get; set; }

        [JsonPropertyName("toPortalId")]
        public string ToPortalId { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("mu")]
        public int MusCaptured { get; set; }
    }

    public class MarkerModel : TaskModel
    {
        [JsonPropertyName("portalId")]
        public string PortalId { get; set; }

        [JsonPropertyName("type")]
        [JsonConverter(typeof(MarkerTypeConverter))]
        public MarkerType Type { get; set; }
    }

    public class BlockerModel : BaseModel
    {
        [JsonPropertyName("ID")]
        public string Id { get; set; }

        [JsonPropertyName("fromPortalId")]
        public string FromPortalId { get; set; }

        [JsonPropertyName("toPortalId")]
        public string ToPortalId { get; set; }

        [JsonPropertyName("assignedTo")]
        public object AssignedTo { get; set; }

        [JsonPropertyName("throwOrderPos")]
        public int ThrowOrderPos { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("completed")]
        public bool Completed { get; set; }
    }

    public class TeamModel : BaseModel
    {
        [JsonPropertyName("opid")]
        public string OperationId { get; set; }

        [JsonPropertyName("teamid")]
        public string TeamId { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("zone")]
        public int Zone { get; set; }
    }

    public class KeysOnHandModel : BaseModel
    {
        [JsonPropertyName("portalId")]
        public string PortalId { get; set; }

        [JsonPropertyName("gid")]
        public string Gid { get; set; }

        [JsonPropertyName("onhand")]
        public int OnHand { get; set; }

        [JsonPropertyName("capsule")]
        public string Capsule { get; set; }
    }

    public class ZoneModel : BaseModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("points")]
        public List<ZonePointModel>? Points { get; set; }
    }

    public class ZonePointModel : BaseModel
    {
        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }

    public enum MarkerType
    {
        DestroyPortal,
        UseVirus,
        CapturePortal,
        FarmPortal,
        LetDecay,
        MeetAgent,
        Other,
        RechargePortal,
        UpgradePortal,
        CreateLink,
        Exclude,
        GetKey,
        GoToPortal
    }
}
