using System;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class TimeSpanExtensions
    {
        public static string ToPrettyString(this TimeSpan timeSpan)
        {
            var result = string.Empty;
            if (timeSpan.Hours > 0)
                result += $"{timeSpan.Hours}";
            else
                result += "00";

            if (timeSpan.Minutes > 0)
                result += $":{timeSpan.Minutes}";
            else
                result += ":00";

            if (timeSpan.Seconds > 0)
                result += $":{timeSpan.Seconds}";
            else
                result += ":00";

            if (string.Equals(result, "00:00:00"))
                result = string.Empty;

            return result;
        }
    }
}