using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Models.JsonConverters;
using System.Collections.Generic;

namespace Rocks.Wasabee.Mobile.Core.Models.Operations
{
#nullable disable
    public class TaskModel : BaseModel
    {
        [JsonProperty("ID")]
        public string Id { get; set; }

        [JsonProperty("assignments")]
        public List<string> Assignments { get; set; } = new();

        [JsonProperty("dependsOn")]
        public List<string> DependsOn { get; set; } = new();

        [JsonProperty("zone")]
        public int Zone { get; set; }

        [JsonProperty("deltaminutes")]
        public int DeltaMinutes { get; set; }

        [JsonProperty("state")]
        [JsonConverter(typeof(TaskStateConverter))]
        public TaskState State { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }
    }

    public enum TaskState
    {
        Pending,
        Acknowledged,
        Assigned,
        Completed
    }
#nullable disable
}