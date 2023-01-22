using LmpMasterServer.Http.Handlers;
using LmpCommon;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Handlers.Compression;
using uhttpsharp.Headers;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace LmpMasterServer.Http
{
    public class LunaHttpServer
    {
        public static HttpServer Server { get; set; } = new HttpServer(new HttpRequestProvider());
        public static ushort Port { get; set; } = 8701;

        public static void Start()
        {
            // With Socket.DualMode listening on IPv6 also listens on IPv4
            var listener = TcpListener.Create(Port);
            Server.Use(new TcpListenerAdapter(listener));

            FileHandler.HttpRootDirectory = WebHandler.BasePath;

            Server.Use(new ExceptionHandler());
            Server.Use(new HeadHandler());
            Server.Use(new CompressionHandler(DeflateCompressor.Default, GZipCompressor.Default));
            Server.Use(new PathGuard(WebHandler.BasePath));
            Server.Use(new FileHandler());
            Server.Use(new HttpRouter()
                .With(string.Empty, new ServerListHandler())
                .With("json", new RestHandler<ServerJson>(new ServerInfoRestHandler(), JsonResponseProvider.Default)));
            // Server.Use((IHttpContext context, Func<Task> next) => { return Task.Run( () =>
            //     {
            //         context.Response.Headers.Append();
            //     } ); });

            Server.Start();
        }
    }

    public class PathGuard : IHttpRequestHandler
    {
        private readonly string basePath;
        public PathGuard(string basepath) => basePath = basepath;

        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            var target = context.Request.Uri.OriginalString.TrimStart('/');
            if (Path.GetFullPath(Path.Combine(basePath, target)).StartsWith(basePath))
            {
                await next().ConfigureAwait(false);
                return;
            }

            context.Response = new HttpResponse(HttpResponseCode.Forbidden, "403 Forbidden", context.Request.Headers.KeepAliveConnection());
        }
    }
}
