using System;
using System.Threading.Tasks;
using uhttpsharp.Clients;

namespace uhttpsharp.Listeners
{
    public interface IHttpListener: IDisposable
    {

        Task<IClient> GetClient();

    }
}
