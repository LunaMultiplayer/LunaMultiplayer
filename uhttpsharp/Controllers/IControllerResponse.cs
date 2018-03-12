using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uhttpsharp.Controllers;

namespace uhttpsharp.Handlers
{
    public interface IControllerResponse
    {
        Task<IHttpResponse> Respond(IHttpContext context, IView view);
    }

    public class CustomResponse : IControllerResponse
    {
        private readonly IHttpResponse _httpResponse;
        public CustomResponse(IHttpResponse httpResponse)
        {
            _httpResponse = httpResponse;
        }
        public Task<IHttpResponse> Respond(IHttpContext context, IView view)
        {
            return Task.FromResult(_httpResponse);
        }
    }

    public class RenderResponse : IControllerResponse
    {
        private readonly HttpResponseCode _code;
        private readonly object _state;
        public RenderResponse(HttpResponseCode code, object state)
        {
            _code = code;
            _state = state;
        }
        public object State
        {
            get { return _state; }
        }
        public HttpResponseCode Code
        {
            get { return _code; }
        }
        public async Task<IHttpResponse> Respond(IHttpContext context, IView view)
        {
            var output = await view.Render(context, _state).ConfigureAwait(false);
            return StringHttpResponse.Create(output.Body, _code, output.ContentType);
        }
    }

    public class RedirectResponse : IControllerResponse
    {
        private readonly Uri _newLocation;
        public RedirectResponse(Uri newLocation)
        {
            _newLocation = newLocation;
        }
        public Task<IHttpResponse> Respond(IHttpContext context, IView view)
        {
            var headers =
                new[]
                {
                    new KeyValuePair<string, string>("Location", _newLocation.ToString())
                };
            return Task.FromResult<IHttpResponse>(
                new HttpResponse(HttpResponseCode.Found, String.Empty, headers,false));
        }
    }

    public static class Response
    {
        public static Task<IControllerResponse> Create(IControllerResponse response)
        {
            return Task.FromResult(response);
        }

        public static Task<IControllerResponse> Custom(IHttpResponse httpResponse)
        {
            return Create(new CustomResponse(httpResponse));
        }
        public static Task<IControllerResponse> Render(HttpResponseCode code, object state)
        {
            return Create(new RenderResponse(code, state));
        }
        public static Task<IControllerResponse> Render(HttpResponseCode code)
        {
            return Create(new RenderResponse(code, null));
        }
        public static Task<IControllerResponse> Redirect(Uri newLocation)
        {
            return Create(new RedirectResponse(newLocation));
        }
    }

    public static class Pipeline
    {
        private class EmptyPipeline : IPipeline
        {
            public Task<IControllerResponse> Go(Func<Task<IControllerResponse>> injectedTask, IHttpContext context)
            {
                return injectedTask();
            }
        }

        public static IPipeline Empty = new EmptyPipeline();
    }
}