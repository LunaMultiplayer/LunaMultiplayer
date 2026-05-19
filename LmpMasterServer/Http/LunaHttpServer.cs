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

            // Exception handler, shows the user an error if an exception occurs
            Server.Use(new ExceptionHandler());

            // Head handler, handles HTTP HEAD requests (like GET but without the content body)
            Server.Use(new HeadHandler());

            // Compression handler
            Server.Use(new CompressionHandler(DeflateCompressor.Default, GZipCompressor.Default));

            // Path guard, blocks path traversal attacks
            Server.Use(new PathGuard(WebHandler.BasePath));

            // File handler, sends files if they're present on the server
            Server.Use(new FileHandler());

            // HTTP router, selects between certain other handlers depending on the path
            var listHandler = new ServerListHandler();
            Server.Use(new HttpRouter()
                .With(string.Empty, listHandler) // List handler, sends the server list
                .With("serverlist", listHandler) // A reference to the same list handler as above
                .With("json", new RestHandler<ServerJson>(new ServerInfoRestHandler(), JsonResponseProvider.Default))); // Sends a JSON version of the server list

            // NotFound handler, returns a 404 page if nothing else.
            Server.Use(new NotFoundHandler());

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
