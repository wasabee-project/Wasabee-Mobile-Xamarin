using Rocks.Wasabee.Core.Models.Converters;
using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.Models.Operations
{
    public class TaskModel : BaseModel
    {
        [JsonPropertyName("ID")]
        public string Id { get; set; }

        [JsonPropertyName("assignments")]
        public List<string>? Assignments { get; set; }

        [JsonPropertyName("dependsOn")]
        public List<string>? DependsOn { get; set; }

        [JsonPropertyName("zone")]
        public int Zone { get; set; }

        [JsonPropertyName("deltaminutes")]
        public int DeltaMinutes { get; set; }

        [JsonPropertyName("state")]
        [JsonConverter(typeof(TaskStateConverter))]
        public TaskState State { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("order")]
        public int Order { get; set; }
    }

    public enum TaskState
    {
        Pending,
        Acknowledged,
        Assigned,
        Completed
    }
}
