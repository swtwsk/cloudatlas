namespace CloudAtlas.Model
{
    public enum PrimaryType {
        Boolean, Contact, Double, Duration, Int, List, Null, Set, String, Time,
    }
    
    public abstract class AttributeType
    {
        public AttributeType(PrimaryType primaryType)
        {
            PrimaryType = primaryType;
        }
        
        public PrimaryType PrimaryType { get; }

        /// <summary>
        /// Indicates whether this type can be implicitly "cast" to given one and vice verse. This is introduced to deal with
        /// null values. In practice, two types are compatible either if they are the same or if one them is a special
        /// "null type".
        /// </summary>
        /// <param name="type">a type to check</param>
        /// <returns>whether two types are compatible with each other</returns>
        public virtual bool IsCompatible(AttributeType type) =>
            PrimaryType == PrimaryType.Null || type.PrimaryType == PrimaryType.Null;

        public virtual bool IsCollection() => false;
    }
}