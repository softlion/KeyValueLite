namespace Vapolia.KeyValueLite
{
    public interface IKeyValueItemSerializer
    {
        T GetValue<T>(KeyValueItem kvi);
        T GetValue<T>(string stringValue);
        string SerializeToString(object value);
    }
}
