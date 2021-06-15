using System.Collections.Generic;
using System.Linq;

namespace Rocks.Wasabee.Mobile.Core.Helpers
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable)
        {
            if (enumerable == null)
                return true;

            if (enumerable is ICollection<T> collection)
            {
                return collection.Count == 0;
            }
            
            return enumerable.Any();
        }
    }

}