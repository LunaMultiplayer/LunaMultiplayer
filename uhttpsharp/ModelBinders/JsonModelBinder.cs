using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using uhttpsharp.Headers;

namespace uhttpsharp.ModelBinders
{
    public class JsonModelBinder : IModelBinder
    {
        private readonly JsonSerializer _serializer;

        public JsonModelBinder(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public JsonModelBinder() : this(JsonSerializer.CreateDefault())
        {
            
        }
        public T Get<T>(byte[] raw, string prefix)
        {
            var rawDecoded = Encoding.UTF8.GetString(raw);

            if (raw.Length == 0)
            {
                return default(T);
            }

            if (prefix == null && typeof(T) == typeof(string))
            {
                return (T)(object)rawDecoded;
            }

            var jToken = JToken.Parse(rawDecoded);

            if (prefix != null)
            {
                jToken = jToken.SelectToken(prefix);
            }

            return jToken.ToObject<T>(_serializer);
        }
        public T Get<T>(IHttpHeaders headers) 
        {
            throw new NotSupportedException();
        }
        public T Get<T>(IHttpHeaders headers, string prefix) 
        {
            throw new NotSupportedException();
        }
    }
}
