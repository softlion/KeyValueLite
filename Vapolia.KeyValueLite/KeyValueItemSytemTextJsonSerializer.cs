using System.Text.Json;

namespace Vapolia.KeyValueLite
{
    public class KeyValueItemSytemTextJsonSerializer : IKeyValueItemSerializer
    {
        //private readonly JsonSerializer serializer;

        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNameCaseInsensitive = true,
        };

        public T GetValue<T>(KeyValueItem kvi)
        {
            if (kvi.Value == null)
                return default;

            return JsonSerializer.Deserialize<T>(kvi.Value, jsonOptions);
        }

        public T GetValue<T>(string stringValue)
        {
            if (stringValue == null)
                return default;

            return JsonSerializer.Deserialize<T>(stringValue, jsonOptions);
        }

        public string SerializeToString(object value)
        {
            if (value == null)
                return null;
            return JsonSerializer.Serialize(value, jsonOptions);
        }
    }
}
