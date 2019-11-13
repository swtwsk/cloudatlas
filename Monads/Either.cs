using System;

namespace CloudAtlas.Monads
{
    public abstract class Either<L, R>
    {
        private Either() {}
        
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
                public override L LeftVal { get; }
                public override R RightVal => throw new InvalidOperationException("There's no Right value in Left");
            }

            public sealed class Right : Either<L, R>
            {
                public Right(R val) => RightVal = val;
                public override L LeftVal => throw new InvalidOperationException("There's no Left value in Right");
                public override R RightVal { get; }
            }
        }
    }
}