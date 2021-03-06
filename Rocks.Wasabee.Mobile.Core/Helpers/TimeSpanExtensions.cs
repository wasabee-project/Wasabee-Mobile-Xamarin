﻿using System;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class TimeSpanExtensions
    {
        public static string ToPrettyString(this TimeSpan timeSpan)
        {
            var result = string.Empty;
            if (timeSpan.Hours > 0)
                result += $"{timeSpan.Hours}h";
            if (timeSpan.Minutes > 0)
            {
                if (!string.IsNullOrEmpty(result))
                    result += " ";

                result += $"{timeSpan.Minutes}min";
            }

            if (string.IsNullOrEmpty(result)) {
                if (timeSpan.Seconds > 0)
                    result += $"{timeSpan.Seconds}sec";
            }

            return result;
        }
    }
}