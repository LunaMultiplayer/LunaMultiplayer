using System.Collections.Generic;
using System.Threading.Tasks;
using uhttpsharp.Handlers;

namespace uhttpsharp.Controllers
{
    public interface IErrorContainer
    {

        void Log(string description);

        IEnumerable<string> Errors { get; }

        bool Any { get; }

        Task<IControllerResponse> GetResponse();

    }
}