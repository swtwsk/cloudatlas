namespace Shared.Model
{
    public class Timestamps
    {
        public PathName PathName { get; private set; }
        public ValueTime Time { get; private set; }
        
        private Timestamps() {}

        public Timestamps(PathName pathName, ValueTime time)
        {
            PathName = pathName;
            Time = time;
        }
    }
}
