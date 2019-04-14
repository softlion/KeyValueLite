using SQLite.Net.Attributes;

namespace Vapolia.KeyValueLite
{
    public class DbMeta
    {
        [PrimaryKey]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
