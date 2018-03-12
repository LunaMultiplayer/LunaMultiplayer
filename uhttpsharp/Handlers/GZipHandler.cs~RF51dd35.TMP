using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using uhttpsharp.Headers;

namespace uhttpsharp.Handlers
{
    public class CompressionHandler : IHttpRequestHandler
    {
        private readonly IEnumerable<ICompressor> _compressors;
        private static readonly char[] Seperator = new [] {','};
        public CompressionHandler(IEnumerable<ICompressor> compressors)
        {
            _compressors = compressors.ToList();
        }

        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            await next();

            if (context.Response == null)
            {
                return;
            }

            var encodings = context.Request.Headers.GetByName("Accept-Encoding")
                .Split(Seperator, StringSplitOptions.RemoveEmptyEntries);


        }
    }

    public interface ICompressor
    {

        string Name { get; }

        IHttpResponse Compress(IHttpResponse response);

    }

    public class DeflateHandler : IHttpRequestHandler
    {
        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            await next();

            if (context.Response != null)
            {
                context.Response = await DeflateResponse.Create(context.Response).ConfigureAwait(false);
            }
        }
    }

    public class DeflateResponse : IHttpResponse
    {
        private readonly HttpResponseCode _responseCode;
        private readonly IHttpHeaders _headers;
        private readonly MemoryStream _memoryStream;
        private readonly bool _closeConnection;

        public DeflateResponse(IHttpResponse child, MemoryStream memoryStream)
        {
            _memoryStream = memoryStream;

            _responseCode = child.ResponseCode;
            _closeConnection = child.CloseConnection;
            _headers =
                new ListHttpHeaders(
                    child.Headers.Where(h => !h.Key.Equals("content-length", StringComparison.InvariantCultureIgnoreCase))
                        .Concat(new[]
                        {
                            new KeyValuePair<string, string>("content-length", memoryStream.Length.ToString(CultureInfo.InvariantCulture)),
                            new KeyValuePair<string, string>("content-encoding", "deflate"), 
                        })
                        .ToList());


        }

        public static async Task<DeflateResponse> Create(IHttpResponse child)
        {
            var memoryStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
            using (var deflateWriter = new StreamWriter(deflateStream))
            {
                await child.WriteBody(deflateWriter).ConfigureAwait(false);
                await deflateWriter.FlushAsync();
            }

            return new DeflateResponse(child, memoryStream);
        }

        public async Task WriteBody(StreamWriter writer)
        {
            _memoryStream.Position = 0;

            await _memoryStream.CopyToAsync(writer.BaseStream).ConfigureAwait(false);
        }
        public HttpResponseCode ResponseCode
        {
            get { return _responseCode; }
        }
        public IHttpHeaders Headers
        {
            get { return _headers; }
        }
        public bool CloseConnection
        {
            get { return _closeConnection; }
        }
    }
}
