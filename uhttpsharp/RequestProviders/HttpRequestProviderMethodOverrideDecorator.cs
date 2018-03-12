using System.IO;
using System.Threading.Tasks;

namespace uhttpsharp.RequestProviders
{
    public class HttpRequestProviderMethodOverrideDecorator : IHttpRequestProvider
    {
        private readonly IHttpRequestProvider _child;

        public HttpRequestProviderMethodOverrideDecorator(IHttpRequestProvider child)
        {
            _child = child;
        }

        public async Task<IHttpRequest> Provide(StreamReader streamReader)
        {
            var childValue = await _child.Provide(streamReader).ConfigureAwait(false);

            if (childValue == null)
            {
                return null;
            }

            string methodName;
            if (!childValue.Headers.TryGetByName("X-HTTP-Method-Override", out methodName))
            {
                return childValue;
            }

            return new HttpRequestMethodDecorator(childValue, HttpMethodProvider.Default.Provide(methodName));
        }
    }
}