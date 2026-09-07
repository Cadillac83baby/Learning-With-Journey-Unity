using System;

namespace LearningWithJourney.UI
{
    // Small compatibility helper for BookReaderControllerV2's generated
    // array Select(...).ToArray() expression without adding a LINQ dependency.
    // This can be removed later if BookReaderControllerV2 imports System.Linq.
    internal static class ArraySelectCompatibilityV2
    {
        public static TResult[] Select<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var result = new TResult[source.Length];
            for (int i = 0; i < source.Length; i++)
                result[i] = selector(source[i]);
            return result;
        }

        public static T[] ToArray<T>(this T[] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source;
        }
    }
}
