using System;
using uhttpsharp.Headers;

namespace uhttpsharp.RequestProviders
{
    internal class HttpRequestMethodDecorator : IHttpRequest
    {
        private readonly IHttpRequest _child;
        private readonly HttpMethods _method;

        public HttpRequestMethodDecorator(IHttpRequest child, HttpMethods method)
        {
            _child = child;
            _method = method;
        }

        public IHttpHeaders Headers
        {
            get { return _child.Headers; }
        }

        public HttpMethods Method
        {
            get { return _method; }
        }

        public string Protocol
        {
            get { return _child.Protocol; }
        }

        public Uri Uri
        {
            get { return _child.Uri; }
        }

        public string[] RequestParameters
        {
            get { return _child.RequestParameters; }
        }

        public IHttpPost Post
        {
            get { return _child.Post; }
        }

        public IHttpHeaders QueryString
        {
            get { return _child.QueryString; }
        }
    }
}
