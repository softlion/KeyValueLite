using System.Collections.Generic;

namespace Vapolia.KeyValueLite
{
    /// <summary>
    /// Equality comparison based on the key of a KeyValueItem.
    /// </summary>
    public sealed class KeyEqualityComparer : IEqualityComparer<KeyValueItem>
    {
        public bool Equals(KeyValueItem x, KeyValueItem y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.Key, y.Key);
        }

        public int GetHashCode(KeyValueItem obj)
        {
            return (obj.Key != null ? obj.Key.GetHashCode() : 0);
        }
    }
}
