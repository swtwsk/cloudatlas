using System;
using Shared.Monads;

namespace Shared.Model
{
    public class Timestamps : IComparable, IComparable<Timestamps>
    {
        public PathName PathName { get; private set; }
        public Maybe<ValueTime> TimeStamp { get; private set; }
        
        private Timestamps() {}

        public Timestamps(PathName pathName, ValueTime timeStamp)
        {
            PathName = pathName;
            TimeStamp = timeStamp?.Just() ?? Maybe<ValueTime>.Nothing;
        }
        
        public Timestamps(PathName pathName, Maybe<ValueTime> timeStamp)
        {
            PathName = pathName;
            TimeStamp = timeStamp;
        }

        public void Deconstruct(out PathName pathName, out Maybe<ValueTime> time)
        {
            pathName = PathName;
            time = TimeStamp;
        }

        public int CompareTo(object obj) => obj == null ? 1 :
            obj is Timestamps other ? CompareTo(other) :
            throw new ArgumentException("Obj is not the same type as instance");

        public int CompareTo(Timestamps other)
        {
            return string.Compare(PathName.ToString(), other.PathName.ToString(), StringComparison.Ordinal);
        }

        public int CompareTimestamps(Timestamps other)
        {
            var isNull = TimeStamp.Match(time => time.IsNull, () => true);
            var (_, oTimeStamp) = other;
            var isOtherNull = oTimeStamp.Match(time => time.IsNull, () => true);
            
            if (isNull)
                return isOtherNull ? 0 : -1;

            if (isOtherNull)
                return 1;
            
            var isLower = ((ValueBoolean) TimeStamp.Val.IsLowerThan(oTimeStamp.Val)).Value.Ref;
            var isEqual = ((ValueBoolean) TimeStamp.Val.IsEqual(oTimeStamp.Val)).Value.Ref;

            return isEqual ? 0 : isLower ? -1 : 1;
        }
    }
}
