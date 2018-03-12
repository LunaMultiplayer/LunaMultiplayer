using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace uhttpsharp.Handlers
{
    public class JsonResponseProvider : IResponseProvider
    {
        public static readonly IResponseProvider Default = new JsonResponseProvider();

        private JsonResponseProvider()
        {
            
        }

        public Task<IHttpResponse> Provide(object value, HttpResponseCode responseCode = HttpResponseCode.Ok)
        {
            var memoryStream = new MemoryStream();
            var writer = new JsonTextWriter(new StreamWriter(memoryStream));
            var serializer = new JsonSerializer() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented};
            serializer.Serialize(writer, value);
            writer.Flush();
            return Task.FromResult<IHttpResponse>(new HttpResponse(responseCode, "application/json; charset=utf-8", memoryStream, true));
        }
    }
}