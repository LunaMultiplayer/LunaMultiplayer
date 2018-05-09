using System;
using uhttpsharp;

namespace LMP.MasterServer.Http
{
    public class HttpException : Exception
    {
        public HttpResponseCode ResponseCode { get; }

        public HttpException(HttpResponseCode responseCode) => ResponseCode = responseCode;

        public HttpException(HttpResponseCode responseCode, string message) : base(message) => ResponseCode = responseCode;
    }
}
