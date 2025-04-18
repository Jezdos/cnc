namespace Core.Extensions
{
    public static class ObjectExtensions
    {
        public static void GetFirstIfPresent<TSource>(this IEnumerable<TSource> source, Action<TSource> action) where TSource : class
        {
            if (source != null && source.Any())
            {
                TSource obj = source.First();
                action(obj);
            }
        }
    }
}
