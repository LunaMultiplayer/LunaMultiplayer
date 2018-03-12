using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace uhttpsharp.Handlers
{
    public class RestHandler<T> : IHttpRequestHandler
    {

        private struct RestCall
        {
            private readonly HttpMethods _method;
            private readonly bool _entryFull;

            public RestCall(HttpMethods method, bool entryFull)
            {
                _method = method;
                _entryFull = entryFull;
            }

            public static RestCall Create(HttpMethods method, bool entryFull)
            {
                return new RestCall(method, entryFull);
            }

            private bool Equals(RestCall other)
            {
                return _method == other._method && _entryFull.Equals(other._entryFull);
            }
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is RestCall && Equals((RestCall)obj);
            }
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)_method*397) ^ _entryFull.GetHashCode();
                }
            }
        }

        private static readonly IDictionary<RestCall, Func<IRestController<T>, IHttpRequest, Task<object>>> RestCallHandlers = new Dictionary<RestCall, Func<IRestController<T>, IHttpRequest, Task<object>>>();

        static RestHandler()
        {
            RestCallHandlers.Add(RestCall.Create(HttpMethods.Get, false), async (c, r) => (object) (await c.Get(r)));
            RestCallHandlers.Add(RestCall.Create(HttpMethods.Get, true), async (c, r) => (object) (await c.GetItem(r)));
            RestCallHandlers.Add(RestCall.Create(HttpMethods.Post, false), async (c, r) => (object) (await c.Create(r)));
            RestCallHandlers.Add(RestCall.Create(HttpMethods.Put, true), async (c, r) => (object) (await c.Upsert(r)));
            RestCallHandlers.Add(RestCall.Create(HttpMethods.Delete, true), async (c, r) => (object) (await c.Delete(r)));
        }

        private readonly IRestController<T> _controller;
        private readonly IResponseProvider _responseProvider;
        public RestHandler(IRestController<T> controller, IResponseProvider responseProvider)
        {
            _controller = controller;
            _responseProvider = responseProvider;
        }

        public async Task Handle(IHttpContext httpContext, Func<Task> next)
        {
            IHttpRequest httpRequest = httpContext.Request;

            var call = new RestCall(httpRequest.Method, httpRequest.RequestParameters.Length > 1);

            Func<IRestController<T>, IHttpRequest, Task<object>> handler;
            if (RestCallHandlers.TryGetValue(call, out handler))
            {
                var value = await handler(_controller, httpRequest).ConfigureAwait(false);
                httpContext.Response = await _responseProvider.Provide(value);

                return;
            }

            await next().ConfigureAwait(false);
        }
    }



}
