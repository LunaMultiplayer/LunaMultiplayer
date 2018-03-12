using System;

namespace uhttpsharp.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class HttpMethodAttribute : Attribute
    {
        private readonly HttpMethods _httpMethod;
        public HttpMethodAttribute(HttpMethods httpMethod)
        {
            _httpMethod = httpMethod;
        }

        public HttpMethods HttpMethod
        {
            get { return _httpMethod; }
        }
    }
}