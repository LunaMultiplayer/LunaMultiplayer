using LunaCommon;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Handlers;

namespace LMP.MasterServer.Http
{
    public class ServerInfoRestController : IRestController<ServerInfo>
    {
        public Task<IEnumerable<ServerInfo>> Get(IHttpRequest request)
        {
            return Task.FromResult(Lidgren.MasterServer.ServerDictionary.Values.Select(s => s.Info));
        }
        public Task<ServerInfo> GetItem(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }

        public Task<ServerInfo> Create(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }
        public Task<ServerInfo> Upsert(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }
        public Task<ServerInfo> Delete(IHttpRequest request)
        {
            throw new HttpException(HttpResponseCode.MethodNotAllowed, "The method is not allowed");
        }
    }
}
