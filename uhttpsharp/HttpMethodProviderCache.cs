using System;
using System.Collections.Concurrent;

namespace uhttpsharp
{
    public class HttpMethodProviderCache : IHttpMethodProvider
    {
        private readonly ConcurrentDictionary<string, HttpMethods> _cache = new ConcurrentDictionary<string, HttpMethods>();

        private readonly Func<String, HttpMethods> _childProvide;
        public HttpMethodProviderCache(IHttpMethodProvider child)
        {
            _childProvide = child.Provide;
        }
        public HttpMethods Provide(string name)
        {
            return _cache.GetOrAdd(name, _childProvide);
        }
    }
}