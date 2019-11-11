using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudAtlas
{
    public static class LinqExtensions
    {
        public static (IEnumerable<T1>, IEnumerable<T2>) Unzip<T, T1, T2>(this IList<T> source,
            Func<T, T1> firstSelector, Func<T, T2> secondSelector)
        {
            var first = source.Select(firstSelector);
            var second = source.Select(secondSelector);
            return (first, second);
        }
    }
}
