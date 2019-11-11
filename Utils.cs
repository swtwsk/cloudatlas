using System;
using System.Collections.Generic;

namespace CloudAtlas
{
    public static class Utils
    {
        private static readonly Random Generator = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Generator.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public static class Compare
    {
        public static IComparer<T> By<T>(Func<T, T, int> comparer) => new DelegateComparer<T>(comparer);

        private class DelegateComparer<T> : IComparer<T>
        {
            private readonly Func<T, T, int> _comparer;

            public DelegateComparer(Func<T, T, int> comparer) => _comparer = comparer;

            public int Compare(T x, T y) => _comparer(x, y);
        }
    }
}