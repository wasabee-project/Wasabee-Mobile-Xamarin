using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Models.Operations
{
    public class OperationModel : BaseModel
    {
        private sealed class OperationModelEqualityComparer : IEqualityComparer<OperationModel>
        {
            public bool Equals(OperationModel x, OperationModel y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.Id, y.Id, StringComparison.InvariantCultureIgnoreCase) && string.Equals(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase) && string.Equals(x.Creator, y.Creator, StringComparison.InvariantCultureIgnoreCase) && string.Equals(x.Color, y.Color, StringComparison.InvariantCultureIgnoreCase) && Equals(x.Portals, y.Portals) && Equals(x.Anchors, y.Anchors) && Equals(x.Links, y.Links) && Equals(x.Blockers, y.Blockers) && Equals(x.Markers, y.Markers) && Equals(x.TeamList, y.TeamList) && Equals(x.KeysOnHand, y.KeysOnHand) && string.Equals(x.Modified, y.Modified, StringComparison.InvariantCultureIgnoreCase) && string.Equals(x.Comment, y.Comment, StringComparison.InvariantCultureIgnoreCase) && string.Equals(x.Fetched, y.Fetched, StringComparison.InvariantCultureIgnoreCase) && x.DownloadedAt.Equals(y.DownloadedAt);
            }

            public int GetHashCode(OperationModel obj)
            {
                unchecked
                {
                    var hashCode = (obj.Id != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Id) : 0);
                    hashCode = (hashCode * 397) ^ (obj.Name != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Name) : 0);
                    hashCode = (hashCode * 397) ^ (obj.Creator != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Creator) : 0);
                    hashCode = (hashCode * 397) ^ (obj.Color != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Color) : 0);
                    hashCode = (hashCode * 397) ^ (obj.Portals != null ? obj.Portals.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Anchors != null ? obj.Anchors.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Links != null ? obj.Links.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Blockers != null ? obj.Blockers.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Markers != null ? obj.Markers.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.TeamList != null ? obj.TeamList.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.KeysOnHand != null ? obj.KeysOnHand.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Modified != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Modified) : 0);
                    hashCode = (hashCode * 397) ^ (obj.Comment != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Comment) : 0);
                    hashCode = (hashCode * 397) ^ (obj.Fetched != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Fetched) : 0);
                    hashCode = (hashCode * 397) ^ obj.DownloadedAt.GetHashCode();
                    return hashCode;
                }
            }
        }

        [JsonIgnore]
        public static IEqualityComparer<OperationModel> OperationModelComparer { get; } = new OperationModelEqualityComparer();

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

        [JsonProperty("assignedToNickname")]
        public string AssignedToNickname { get; set; }

        [JsonProperty("throwOrderPos")]
        public int ThrowOrderPos { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }
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

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("completedBy")]
        public string CompletedBy { get; set; }

        [JsonProperty("assignedTo")]
        public string AssignedTo { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }
    }

    public class TeamModel : BaseModel
    {
        [JsonProperty("teamid")]
        public string TeamId { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
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

}