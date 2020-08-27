using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Models.Operations
{
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

        [JsonProperty("modified")]
        public string Modified { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("fetched")]
        public string Fetched { get; set; }

        [JsonIgnore]
        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }

    public class PortalModel
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

    public class LinkModel
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

        [JsonProperty("assignedToNickname")]
        public string AssignedToNickname { get; set; }

        [JsonProperty("throwOrderPos")]
        public int ThrowOrderPos { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
    }

    public class BlockerModel
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

    public class MarkerModel
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("portalId")]
        public string PortalId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("completedBy")]
        public string CompletedBy { get; set; }

        [JsonProperty("assignedTo")]
        public string AssignedTo { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }
    }

    public class TeamModel
    {
        [JsonProperty("teamid")]
        public string TeamId { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }

    public class KeysOnHandModel
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

}