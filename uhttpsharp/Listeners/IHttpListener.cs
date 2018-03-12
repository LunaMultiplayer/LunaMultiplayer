using System.Threading.Tasks;
using uhttpsharp.Clients;

namespace uhttpsharp.Listeners
{
    public interface IHttpListener
    {

        Task<IClient> GetClient();

    }
}
