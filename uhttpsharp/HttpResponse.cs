/*
 * Copyright (C) 2011 uhttpsharp project - http://github.com/raistlinthewiz/uhttpsharp
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.

 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uhttpsharp.Headers;

namespace uhttpsharp
{
    public interface IHttpResponse
    {
        Task WriteBody(StreamWriter writer);

        /// <summary>
        /// Gets the status line of this http response,
        /// The first line that will be sent to the client.
        /// </summary>
        HttpResponseCode ResponseCode { get; }

        IHttpHeaders Headers { get; }

        bool CloseConnection { get; }
    }

    public abstract class HttpResponseBase : IHttpResponse
    {
        private readonly HttpResponseCode _code;
        private readonly IHttpHeaders _headers;
        protected HttpResponseBase(HttpResponseCode code, IHttpHeaders headers)
        {
            _code = code;
            _headers = headers;
        }

        public abstract Task WriteBody(StreamWriter writer);
        public HttpResponseCode ResponseCode
        {
            get { return _code; }
        }
        public IHttpHeaders Headers
        {
            get { return _headers; }
        }
        public bool CloseConnection
        {
            get
            {
                string value;
                return !(_headers.TryGetByName("Connection", out value) &&
                         value.Equals("Keep-Alive", StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }

    public sealed class StreamHttpResponse : HttpResponseBase
    {
        private readonly Stream _body;
        public StreamHttpResponse(Stream body, HttpResponseCode code, IHttpHeaders headers) : base(code, headers)
        {
            _body = body;
        }

        public static IHttpResponse Create(Stream body, HttpResponseCode code = HttpResponseCode.Ok, string contentType = "text/html; charset=utf-8", bool keepAlive = true)
        {
            return new StreamHttpResponse(body, code, new ListHttpHeaders(new[]
            {
                new KeyValuePair<string, string>("Date", DateTime.UtcNow.ToString("R")),
                new KeyValuePair<string, string>("content-type", contentType),
                new KeyValuePair<string, string>("connection", keepAlive ? "keep-alive" : "close"), 
                new KeyValuePair<string, string>("content-length", body.Length.ToString(CultureInfo.InvariantCulture)), 
            }));
        }

        public async override Task WriteBody(StreamWriter writer)
        {
            await writer.FlushAsync().ConfigureAwait(false);
            await _body.CopyToAsync(writer.BaseStream).ConfigureAwait(false);
            await writer.BaseStream.FlushAsync().ConfigureAwait(false);
        }
    }

    public sealed class EmptyHttpResponse : HttpResponseBase
    {
        
        public EmptyHttpResponse(HttpResponseCode code, IHttpHeaders headers) : base(code, headers)
        {
            
        }

        public static IHttpResponse Create(HttpResponseCode code = HttpResponseCode.Ok, bool keepAlive = true)
        {
            return new EmptyHttpResponse(code, new ListHttpHeaders(new[]
            {
                new KeyValuePair<string, string>("Date", DateTime.UtcNow.ToString("R")),
                new KeyValuePair<string, string>("content-type", "text/html"),
                new KeyValuePair<string, string>("connection", keepAlive ? "keep-alive" : "close"), 
                new KeyValuePair<string, string>("content-length", "0"), 
            }));
        }

        public override Task WriteBody(StreamWriter writer)
        {
            return Task.Factory.GetCompleted();
        }
    }

    public sealed class StringHttpResponse : HttpResponseBase
    {
        private readonly string _body;
        
        public StringHttpResponse(string body, HttpResponseCode code, IHttpHeaders headers) : base(code, headers)
        {
            _body = body;
        }

        public static IHttpResponse Create(string body, HttpResponseCode code = HttpResponseCode.Ok, string contentType = "text/html; charset=utf-8", bool keepAlive = true)
        {
            return new StringHttpResponse(body, code, new ListHttpHeaders(new[]
            {
                new KeyValuePair<string, string>("Date", DateTime.UtcNow.ToString("R")),
                new KeyValuePair<string, string>("content-type", contentType),
                new KeyValuePair<string, string>("connection", keepAlive ? "keep-alive" : "close"), 
                new KeyValuePair<string, string>("content-length", Encoding.UTF8.GetByteCount(body).ToString(CultureInfo.InvariantCulture)), 
            }));
        }

        public async override Task WriteBody(StreamWriter writer)
        {
            await writer.WriteAsync(_body).ConfigureAwait(false);
        }
        
    }

    public sealed class HttpResponse : IHttpResponse
    {
        private Stream ContentStream { get; set; }

        private readonly Stream _headerStream = new MemoryStream();
        private readonly bool _closeConnection;
        private readonly IHttpHeaders _headers;
        private readonly HttpResponseCode _responseCode;

        public HttpResponse(HttpResponseCode code, string content, bool closeConnection)
            : this(code, "text/html; charset=utf-8", StringToStream(content), closeConnection)
        {
        }

        public HttpResponse(HttpResponseCode code, string content, IEnumerable<KeyValuePair<string,string>> headers, bool closeConnection)
            : this(code, "text/html; charset=utf-8", StringToStream(content), closeConnection,headers)
        {
        }

        public HttpResponse(string contentType, Stream contentStream, bool closeConnection)
            : this(HttpResponseCode.Ok, contentType, contentStream, closeConnection)
        {
        }

        public HttpResponse(HttpResponseCode code, string contentType, Stream contentStream, bool keepAliveConnection, IEnumerable<KeyValuePair<string, string>> headers)
        {
            ContentStream = contentStream;

            _closeConnection = !keepAliveConnection;

            _responseCode = code;
            _headers = new ListHttpHeaders(new[]
            {
                new KeyValuePair<string, string>("Date", DateTime.UtcNow.ToString("R")),
                new KeyValuePair<string, string>("Connection", _closeConnection ? "Close" : "Keep-Alive"),
                new KeyValuePair<string, string>("Content-Type", contentType),
                new KeyValuePair<string, string>("Content-Length", ContentStream.Length.ToString(CultureInfo.InvariantCulture)),
            }.Concat(headers).ToList());

        }
        public HttpResponse(HttpResponseCode code, string contentType, Stream contentStream, bool keepAliveConnection) :
            this(code, contentType, contentStream, keepAliveConnection, Enumerable.Empty<KeyValuePair<string, string>>())
        {
        }


        public HttpResponse(HttpResponseCode code, byte[] contentStream, bool keepAliveConnection) 
            : this (code, "text/html; charset=utf-8", new MemoryStream(contentStream), keepAliveConnection)
        {
        }
        
        public HttpResponse(HttpResponseCode code, string contentType, string content, bool closeConnection)
            : this(code, contentType, StringToStream(content), closeConnection)
        {
        }

        public static HttpResponse CreateWithMessage(HttpResponseCode code, string message, bool keepAliveConnection, string body = "")
        {
            return new HttpResponse(
                code,
                string.Format(
                    "<html><head><title>{0}</title></head><body><h1>{0}</h1><hr>{1}</body></html>",
                    message, body), keepAliveConnection);
        }
        private static MemoryStream StringToStream(string content)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            return stream;
        }
        public async Task WriteBody(StreamWriter writer)
        {
            ContentStream.Position = 0;
            await ContentStream.CopyToAsync(writer.BaseStream).ConfigureAwait(false);            
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

        public async Task WriteHeaders(StreamWriter writer)
        {
            _headerStream.Position = 0;
            await _headerStream.CopyToAsync(writer.BaseStream).ConfigureAwait(false);
            
        }
    }
}
