using Server.Web.Structures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Handlers;

namespace Server.Web.Handlers
{
    public class ServerInformationRestController : IRestController<ServerInformation>
    {
        public Task<IEnumerable<ServerInformation>> Get(IHttpRequest request)
        {
            return Task.FromResult(new[] { WebServer.ServerInformation }.AsEnumerable());
        }

        public Task<ServerInformation> GetItem(IHttpRequest request)
        {
            return Task.FromResult(WebServer.ServerInformation);
        }

        public Task<ServerInformation> Create(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }
        public Task<ServerInformation> Upsert(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }
        public Task<ServerInformation> Delete(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }
    }
}
