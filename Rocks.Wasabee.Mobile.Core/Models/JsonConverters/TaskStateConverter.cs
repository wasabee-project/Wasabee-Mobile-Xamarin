using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using System;

namespace Rocks.Wasabee.Mobile.Core.Models.JsonConverters
{
    public class TaskStateConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
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

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}