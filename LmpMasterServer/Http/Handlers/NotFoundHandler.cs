using System;
using System.Threading.Tasks;
using uhttpsharp;

namespace LmpMasterServer.Http.Handlers
{
    public class NotFoundHandler : IHttpRequestHandler
    {
        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            context.Response = new HttpResponse(HttpResponseCode.NotFound, "404 Not Found", false);
        }
    }
}
