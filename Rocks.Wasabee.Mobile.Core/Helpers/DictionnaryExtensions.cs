using System.Collections.Generic;
using System.Linq;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class DictionnaryExtensions
    {
        public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(", ", dictionary.Select(kv => kv.Key + "=" + kv.Value)) + "}";
        }
    }
}