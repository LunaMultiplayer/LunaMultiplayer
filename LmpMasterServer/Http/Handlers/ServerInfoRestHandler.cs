using LmpCommon;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Handlers;

namespace LmpMasterServer.Http.Handlers
{
    public class ServerInfoRestHandler : IRestController<ServerJson>
    {
        public Task<IEnumerable<ServerJson>> Get(IHttpRequest request)
        {
            return Task.FromResult(Lidgren.MasterServer.ServerDictionary.Values.Select(s => new ServerJson(s)));
        }
        public Task<ServerJson> GetItem(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }

        public Task<ServerJson> Create(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }
        public Task<ServerJson> Upsert(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }
        public Task<ServerJson> Delete(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }
    }
}
