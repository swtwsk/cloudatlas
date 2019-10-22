namespace CloudAtlas
{
    public class RefStruct<T>
        where T : struct
    {
        public T Ref { get; set; }
        
        public static implicit operator RefStruct<T>(T value) => new RefStruct<T> {Ref = value};

        public static implicit operator T?(RefStruct<T> value) => value?.Ref;

        public static implicit operator RefStruct<T>(T? value) =>
            value.HasValue ? new RefStruct<T> {Ref = value.Value} : null;
        
        public override string ToString() => Ref.ToString();
    }

    public static class RefStructExtensions
    {
        public static RefStruct<T> ToNullableWrapper<T>(this T value) where T : struct =>
            new RefStruct<T> {Ref = value};
    }
}