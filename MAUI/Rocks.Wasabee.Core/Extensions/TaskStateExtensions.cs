using Rocks.Wasabee.Core.Models.Operations;
using Rocks.Wasabee.Core.Resources.I18n;

namespace Rocks.Wasabee.Core.Extensions
{
    public static class TaskStateExtensions
    {
        public static string ToFriendlyString(this TaskState taskState)
        {
            return taskState switch
            {
                TaskState.Pending => Strings.TaskState_Pending,
                TaskState.Acknowledged => Strings.TaskState_Acknowledged,
                TaskState.Assigned => Strings.TaskState_Assigned,
                TaskState.Completed => Strings.TaskState_Completed,
                _ => throw new ArgumentOutOfRangeException(taskState.ToString())
            };
        }
    }
}