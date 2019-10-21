namespace CloudAtlas
{
    public class NullableWrapper<T>
        where T : struct
    {
        public T Wrapped { get; set; }
        
        public static implicit operator NullableWrapper<T>(T value) => new NullableWrapper<T> {Wrapped = value};

        public static implicit operator T?(NullableWrapper<T> value) => value?.Wrapped;

        public static implicit operator NullableWrapper<T>(T? value) =>
            value.HasValue ? new NullableWrapper<T> {Wrapped = value.Value} : null;

        public override string ToString() => Wrapped.ToString();
    }

    public static class NullableWrapperExtensions
    {
        public static NullableWrapper<T> ToNullableWrapper<T>(this T value) where T : struct =>
            new NullableWrapper<T> {Wrapped = value};
    }
}