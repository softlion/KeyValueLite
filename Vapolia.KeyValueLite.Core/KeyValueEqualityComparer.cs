using System.Collections.Generic;

namespace Vapolia.KeyValueLite
{
    /// <summary>
    /// Equality comparison based on the key and value of a KeyValueItem.
    /// </summary>
    public sealed class KeyValueEqualityComparer : IEqualityComparer<KeyValueItem>
    {
        public bool Equals(KeyValueItem x, KeyValueItem y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.Key, y.Key) && Equals(x.Value, y.Value);
        }

        public int GetHashCode(KeyValueItem obj)
        {
            unchecked
            {
                return ((obj.Key != null ? obj.Key.GetHashCode() : 0) * 397) ^ (obj.Value != null ? obj.Value.GetHashCode() : 0);
            }
        }
    }
}
