using Rocks.Wasabee.Mobile.Core.Helpers.Xaml;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using System;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class TaskStateExtensions
    {
        public static string ToFriendlyString(this TaskState taskState)
        {
            return taskState switch
            {
                TaskState.Pending => TranslateExtension.GetValue("TaskState_Pending"),
                TaskState.Acknowledged => TranslateExtension.GetValue("TaskState_Acknowledged"),
                TaskState.Assigned => TranslateExtension.GetValue("TaskState_Assigned"),
                TaskState.Completed => TranslateExtension.GetValue("TaskState_Completed"),
                _ => throw new ArgumentOutOfRangeException(taskState.ToString())
            };
        }
    }
}