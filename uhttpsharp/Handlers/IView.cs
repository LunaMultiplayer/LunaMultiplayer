using System.Threading.Tasks;
using Newtonsoft.Json;

namespace uhttpsharp.Handlers
{
    public interface IViewResponse
    {

        string Body { get; }

        string ContentType { get; }

    }

    public interface IView
    {
        Task<IViewResponse> Render (IHttpContext context, object state);
    }

    public class JsonView : IView
    {
        public Task<IViewResponse> Render (IHttpContext context, object state)
        {
            return Task.FromResult<IViewResponse>(new JsonViewResponse(JsonConvert.SerializeObject(state)));
        }

        class JsonViewResponse : IViewResponse
        {
            private readonly string _body;
            public JsonViewResponse(string body)
            {
                _body = body;
            }
            public string Body
            {
                get { return _body; }
            }
            public string ContentType
            {
                get { return "application/json; charset=utf-8"; }
            }
        }
    }
}