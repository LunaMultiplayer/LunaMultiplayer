using Newtonsoft.Json;
using System;
using System.Net;

namespace uhttpsharp.Json
{
    public class IPEndPointConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPEndPoint));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var endpoint = (IPEndPoint)value;
            writer.WriteValue($"{endpoint.Address}:{endpoint.Port}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var elements = ((string) reader.Value).Split(':');
            return new IPEndPoint(IPAddress.Parse(elements[0]), int.Parse(elements[1]));
        }
    }
}
