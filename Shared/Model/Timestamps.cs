using System;

namespace Shared.Model
{
    public class Timestamps : IComparable, IComparable<Timestamps>
    {
        public PathName PathName { get; private set; }
        public ValueTime TimeStamp { get; private set; }
        
        private Timestamps() {}

        public Timestamps(PathName pathName, ValueTime timeStamp)
        {
            PathName = pathName;
            TimeStamp = timeStamp;
        }

        public void Deconstruct(out PathName pathName, out ValueTime time)
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

        public int CompareTimestamps(Timestamps other) => TimeStamp.CompareTo(other.TimeStamp);

        // map as `void Apply` due to performance reasons
        public void ApplyFunc(Func<ValueTime, ValueTime> mapFunc)
        {
            TimeStamp = mapFunc(TimeStamp);
        }
    }
}
