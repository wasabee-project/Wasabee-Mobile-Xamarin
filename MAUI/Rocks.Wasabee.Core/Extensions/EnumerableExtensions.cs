namespace Rocks.Wasabee.Core.Extensions
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

        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? enumerable)
        {
            return !IsNullOrEmpty(enumerable);
        }
    }
}