using System;
using Rocks.Wasabee.Mobile.Core.Models.Operations;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class TaskStateExtensions
    {
        public static string ToFriendlyString(this TaskState taskState)
        {
            return taskState switch
            {
                TaskState.Pending => "Pending",
                TaskState.Acknowledged => "Acknowledged",
                TaskState.Assigned => "Assigned",
                TaskState.Completed => "Completed",
                _ => throw new ArgumentOutOfRangeException(taskState.ToString())
            };
        }
    }
}