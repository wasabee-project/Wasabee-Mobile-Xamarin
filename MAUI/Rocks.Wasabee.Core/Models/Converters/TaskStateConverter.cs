using Rocks.Wasabee.Core.Models.Operations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rocks.Wasabee.Core.Models.Converters
{
    public class TaskStateConverter : JsonConverter<TaskState>
    {
        public override void Write(Utf8JsonWriter writer, TaskState value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override TaskState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
                return TaskState.Pending;

            return value switch
            {
                "pending" => TaskState.Pending,
                "assigned" => TaskState.Assigned,
                "acknowledged" => TaskState.Acknowledged,
                "completed" => TaskState.Completed,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
