using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Models.Operations
{
#nullable disable
    public class OperationModel : BaseModel
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("opportals")]
        public List<PortalModel> Portals { get; set; }

        [JsonProperty("anchors")]
        public List<string> Anchors { get; set; }

        [JsonProperty("links")]
        public List<LinkModel> Links { get; set; }

        [JsonProperty("blockers")]
        public List<BlockerModel> Blockers { get; set; }

        [JsonProperty("markers")]
        public List<MarkerModel> Markers { get; set; }

        [JsonProperty("teamlist")]
        public List<TeamModel> TeamList { get; set; }

        [JsonProperty("keysonhand")]
        public List<KeysOnHandModel> KeysOnHand { get; set; }

        [JsonProperty("zones")]
        public List<ZoneModel> Zones { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("modified")]
        public string Modified { get; set; }

        [JsonProperty("lasteditid")]
        public string LastEditID { get; set; }

        [JsonIgnore]
        public bool IsHiddenLocally { get; set; } = false;

        [JsonIgnore]
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }

    public class PortalModel : BaseModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lat")]
        public string Lat { get; set; }

        [JsonProperty("lng")]
        public string Lng { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("hardness")]
        public string Hardness { get; set; }
    }

    public class LinkModel : BaseModel
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("fromPortalId")]
        public string FromPortalId { get; set; }

        [JsonProperty("toPortalId")]
        public string ToPortalId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("assignedTo")]
        public string AssignedTo { get; set; }

        [JsonProperty("throwOrderPos")]
        public int ThrowOrderPos { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("zone")]
        public int Zone { get; set; }
    }

    public class BlockerModel : BaseModel
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("fromPortalId")]
        public string FromPortalId { get; set; }

        [JsonProperty("toPortalId")]
        public string ToPortalId { get; set; }

        [JsonProperty("assignedTo")]
        public object AssignedTo { get; set; }

        [JsonProperty("throwOrderPos")]
        public int ThrowOrderPos { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }
    }

    public class MarkerModel : BaseModel
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("portalId")]
        public string PortalId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("assignedTo")]
        public string AssignedTo { get; set; }

        [JsonProperty("assignedTeam")]
        public string AssignedTeam { get; set; }

        [JsonProperty("completedID")]
        public string CompletedId { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("zone")]
        public int Zone { get; set; }
    }

    public class TeamModel : BaseModel
    {
        [JsonProperty("opid")]
        public string OperationId { get; set; }

        [JsonProperty("teamid")]
        public string TeamId { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("zone")]
        public int Zone { get; set; }
    }

    public class KeysOnHandModel : BaseModel
    {
        [JsonProperty("portalId")]
        public string PortalId { get; set; }

        [JsonProperty("gid")]
        public string Gid { get; set; }

        [JsonProperty("onhand")]
        public int OnHand { get; set; }

        [JsonProperty("capsule")]
        public string Capsule { get; set; }
    }

    public class ZoneModel : BaseModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("points")]
        public List<ZonePointModel> Points { get; set; }
    }

    public class ZonePointModel : BaseModel
    {
        [JsonProperty("position")]
        public int Position { get; set; }
        
        [JsonProperty("lat")]
        public string Lat { get; set; }
        
        [JsonProperty("lng")]
        public string Lng { get; set; }
    }
#nullable enable
}