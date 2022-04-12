using LmpMasterServer.Http.Handlers;
using LmpCommon;
using System.Net;
using System.Net.Sockets;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Handlers.Compression;
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
            // Due to Socket.DualMode (default true) listening on IPv6 also listens on IPv4
            var listener = new TcpListener(Socket.OSSupportsIPv6 ? IPAddress.IPv6Any : IPAddress.Any, Port);
            Server.Use(new TcpListenerAdapter(listener));

            Server.Use(new ExceptionHandler());
            Server.Use(new HeadHandler());
            Server.Use(new CompressionHandler(DeflateCompressor.Default, GZipCompressor.Default));
            Server.Use(new FileHandler());
            Server.Use(new HttpRouter()
                .With(string.Empty, new ServerListHandler())
                .With("json", new RestHandler<ServerJson>(new ServerInfoRestHandler(), JsonResponseProvider.Default)));

            Server.Start();
        }
    }
}
