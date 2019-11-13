using System;
using System.Collections.Generic;

namespace CloudAtlas.Monads
{
    public abstract class Maybe<T>
    {
        public abstract bool HasValue { get; }
        public bool HasNothing => !HasValue;

        public abstract T Val { get; }
        
        private Maybe() {}
        
        public static Maybe<T> Just(T value) => new Data.Just(value);
        public static Maybe<T> Nothing => new Data.Nothing();

        public TRes Match<TRes>(Func<T, TRes> justFunc, Func<TRes> noneFunc) =>
            this switch
            {
                Data.Just just => justFunc(just.Val),
                Data.Nothing _ => noneFunc(),
                _ => throw new NotSupportedException("Unexpected exception")
            };

        public Maybe<T2> FMap<T2>(Func<T, T2> func) =>
            this switch
            {
                Data.Just just => Maybe<T2>.Just(func(just.Val)),
                Data.Nothing _ => Maybe<T2>.Nothing,
                _ => throw new NotSupportedException("Unexpected exception")
            };

        public Maybe<T2> Bind<T2>(Func<T, Maybe<T2>> func) =>
            this switch
            {
                Data.Just just => func(just.Val),
                Data.Nothing _ => Maybe<T2>.Nothing,
                _ => throw new NotSupportedException("Unexpected exception")
            };

        public Maybe<(T, T2)> Zip<T2>(Maybe<T2> snd) => Bind(fst => snd.Bind(snd => (fst, snd).Just()));

        private static class Data
        {
            public sealed class Just : Maybe<T>
            {
                public override T Val { get; }
                public Just(T val) => Val = val;
                public override bool HasValue => true;
            }

            public sealed class Nothing : Maybe<T>
            {
                public override bool HasValue => false;
                public override T Val => throw new InvalidOperationException("There's no value in Nothing");
            }
        }
    }

    public static class MaybeExtensions
    {
        public static Maybe<T> Just<T>(this T that) => Maybe<T>.Just(that);

        public static Maybe<List<T>> Sequence<T>(this IEnumerable<Maybe<T>> that)
        {
            var list = new List<T>();
            foreach (var maybe in that)
            {
                if (maybe.HasNothing)
                    return Maybe<List<T>>.Nothing;
                list.Add(maybe.Val);
            }
            return list.Just();
        }
    }
}