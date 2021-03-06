using System.Globalization;

namespace Shared
{
    public class RefStruct<T>
        where T : struct
    {
        public T Ref { get; set; }

        private RefStruct() {}
        public RefStruct(T val) => Ref = val;

        public static implicit operator RefStruct<T>(T value) => new RefStruct<T>(value);

        public static implicit operator T?(RefStruct<T> value) => value?.Ref;

        public static implicit operator RefStruct<T>(T? value) =>
            value.HasValue ? new RefStruct<T>(value.Value) : null;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Ref.Equals(((RefStruct<T>) obj).Ref);
        }

        protected bool Equals(RefStruct<T> other) => Ref.Equals(other.Ref);

        public override int GetHashCode() => Ref.GetHashCode();

        public override string ToString()
        {
            return Ref is double d ? d.ToString("0.0###", CultureInfo.InvariantCulture) : Ref.ToString().ToLower();
        }
    }

    public static class RefStructExtensions
    {
        public static RefStruct<T> ToNullableWrapper<T>(this T value) where T : struct =>
            new RefStruct<T>(value);
    }
}