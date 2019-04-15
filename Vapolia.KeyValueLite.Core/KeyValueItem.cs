using System;
using SQLite.Net2;

namespace Vapolia.KeyValueLite
{
    /// <summary>
    /// Caching strategy v1:
    /// - only first page of full lists are cached
    /// => ObjectType is a full list
    /// => ObjectId is not used
    /// </summary>
    [Preserve(AllMembers = true)]
    public class KeyValueItem : IComparable, IComparable<KeyValueItem>
    {
        [PrimaryKey]
        public string Key { get; set; }

        /// <summary>
        /// Date of expiration
        /// </summary>
        [Indexed]
        public DateTimeOffset? ExpiresOn { get; set; }

        [Indexed]
        public string ValueType { get; set; }


        public string Value { get; set; }

        /// <summary>
        /// Required
        /// </summary>
        public KeyValueItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueItem"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiresOn">The expire date after which this key is no longer valid.</param>
        public KeyValueItem(in string key, in string value, in string valueType, in DateTimeOffset? expiresOn = null)
        {
            Key = key;
            Value = value;
            ValueType = valueType;
            ExpiresOn = expiresOn;
        }

        #region Equality & Comparison Methods

        public int CompareTo(KeyValueItem other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Key, other.Key, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            if (!(obj is KeyValueItem)) throw new ArgumentException($"Object must be of type {nameof(KeyValueItem)}");
            return CompareTo((KeyValueItem) obj);
        }

        protected bool Equals(KeyValueItem other)
        {
            return string.Equals(Key, other.Key) && Equals(Value, other.Value) && ExpiresOn?.Equals(other.ExpiresOn) == true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((KeyValueItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Key != null ? Key.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExpiresOn?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Key}={Value}";
        }

        #endregion
    }
}
