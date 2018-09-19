using System;
using System.Threading.Tasks;
using uhttpsharp;

namespace LmpMasterServer.Http.Handlers
{
    public class HeadHandler : IHttpRequestHandler
    {
        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            if (context.Request.Method == HttpMethods.Head)
            {
                context.Response = new HttpResponse(HttpResponseCode.Ok, string.Empty, false);
                return;
            }

            await next().ConfigureAwait(false);
        }
    }
}
