//using System;
//using System.IO;
//using System.Text;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Serialization;

//namespace Vapolia.KeyValueLite
//{

//    public class KeyValueItemNewtonsoftJsonSerializer : IKeyValueItemSerializer
//    {
//        private readonly JsonSerializer serializer;

//        private readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
//        {
//            Culture = System.Globalization.CultureInfo.InvariantCulture,
//            NullValueHandling = NullValueHandling.Ignore,
//            ObjectCreationHandling = ObjectCreationHandling.Replace,
//            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
//            TypeNameHandling = TypeNameHandling.All,
//            ContractResolver = new DateTimeHighPrecisionContractResolver() //Try to fix akavache infinite loop problem
//        };

//        public KeyValueItemNewtonsoftJsonSerializer()
//        {
//            serializer = JsonSerializer.Create(JsonSettings);
//        }

//        public T GetValue<T>(KeyValueItem kvi)
//        {
//            if (kvi.Value == null)
//                return default;

//            return serializer.Deserialize<T>(new JsonTextReader(new StringReader(kvi.Value)));
//        }

//        public T GetValue<T>(string stringValue)
//        {
//            if (stringValue == null)
//                return default;

//            return serializer.Deserialize<T>(new JsonTextReader(new StringReader(stringValue)));
//        }

//        public string SerializeToString(object value)
//        {
//            if (value == null)
//                return null;
//            var sb = new StringBuilder();
//            serializer.Serialize(new JsonTextWriter(new StringWriter(sb)), value);
//            return sb.ToString();
//        }
//    }

//    class DateTimeHighPrecisionContractResolver : DefaultContractResolver
//    {
//        protected override JsonContract CreateContract(Type objectType)
//        {
//            var contract = base.CreateContract(objectType);
//            if (objectType == typeof(DateTime) || objectType == typeof(DateTime?) || objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?))
//                contract.Converter = DateTimeHighPrecisionJsonConverter.Instance;
//            return contract;
//        }
//    }

//    class DateTimeHighPrecisionJsonConverter : JsonConverter
//    {
//        public static readonly DateTimeHighPrecisionJsonConverter Instance = new DateTimeHighPrecisionJsonConverter();

//        public override bool CanConvert(Type objectType)
//        {
//            return objectType == typeof(DateTime) || objectType == typeof(DateTime?) || objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?);
//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            if (reader.TokenType != JsonToken.StartObject)
//                return null;

//            var ds = (DateStruct)serializer.Deserialize(reader, typeof(DateStruct));

//            return new DateTimeOffset(ds.DateTicks, new TimeSpan(ds.OffsetTicks));
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            if (value != null)
//            {
//                var dateTimeOffset =
//                    value is DateTimeOffset dto2 ? dto2
//                    : value is DateTimeOffset dto ? dto
//                    : value is DateTime dt ? dt
//                    : ((DateTime?)value).Value;

//                serializer.Serialize(writer, new DateStruct { DateTicks = dateTimeOffset.ToUniversalTime().Ticks, OffsetTicks = dateTimeOffset.Offset.Ticks });
//            }
//        }

//        class DateStruct
//        { 
//            public long DateTicks { get;set;}
//            public long OffsetTicks { get; set; }
//        }
//    }
//}
