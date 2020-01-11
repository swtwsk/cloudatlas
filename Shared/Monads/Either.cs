using System;
using System.Collections.Generic;

namespace Shared.Monads
{
    public abstract class Either<L, R>
    {
        private Either() {}
        
        public abstract bool IsRight { get; }
        public bool IsLeft => !IsRight;
        
        public abstract L LeftVal { get; }
        public abstract R RightVal { get; }
        
        public static Either<L, R> Left(L val) => new Data.Left(val); 
        public static Either<L, R> Right(R val) => new Data.Right(val);

        public Either<L, R2> FMap<R2>(Func<R, R2> func) =>
            this switch
            {
                Data.Left left => Either<L, R2>.Left(left.LeftVal),
                Data.Right right => Either<L, R2>.Right(func(right.RightVal)),
                _ => throw new NotSupportedException("Unexpected exception")
            };

        public Either<L2, R2> Bimap<L2, R2>(Func<L, L2> leftFunc, Func<R, R2> rightFunc) =>
            this switch
            {
                Data.Left left => Either<L2, R2>.Left(leftFunc(left.LeftVal)),
                Data.Right right => Either<L2, R2>.Right(rightFunc(right.RightVal)),
                _ => throw new NotSupportedException("Unexpected exception")
            };

        public Either<L, R2> Bind<R2>(Func<R, Either<L, R2>> func) =>
            this switch
            {
                Data.Left left => Either<L, R2>.Left(left.LeftVal),
                Data.Right right => func(right.RightVal),
                _ => throw new NotSupportedException("Unexpected exception")
            };

        private static class Data
        {
            public sealed class Left : Either<L, R>
            {
                public Left(L val) => LeftVal = val;
                public override bool IsRight { get; } = false;
                public override L LeftVal { get; }
                public override R RightVal => throw new InvalidOperationException("There's no Right value in Left");
            }

            public sealed class Right : Either<L, R>
            {
                public Right(R val) => RightVal = val;
                public override bool IsRight { get; } = true;
                public override L LeftVal => throw new InvalidOperationException("There's no Left value in Right");
                public override R RightVal { get; }
            }
        }
    }
    
    public static class EitherExtensions
    {
        public static Either<L, R> Left<L, R>(this L that) => Either<L, R>.Left(that);
        public static Either<L, R> Right<L, R>(this R that) => Either<L, R>.Right(that);

        public static Either<L, List<R>> Sequence<L, R>(this IEnumerable<Either<L, R>> that)
        {
            var list = new List<R>();
            foreach (var either in that)
            {
                if (either.IsLeft)
                    return Either<L, List<R>>.Left(either.LeftVal);
                list.Add(either.RightVal);
            }
            return list.Right<L, List<R>>();
        }
    }
}