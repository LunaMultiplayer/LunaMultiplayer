using System.Threading.Tasks;

namespace uhttpsharp.Handlers
{
    public interface IResponseProvider
    {

        Task<IHttpResponse> Provide(object value, HttpResponseCode responseCode = HttpResponseCode.Ok);

    }
}