using LunaCommon;
using System.Net;
using System.Net.Sockets;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Handlers.Compression;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace LMP.MasterServer.Http
{
    public class LunaHttpServer
    {
        public static HttpServer Server { get; set; } = new HttpServer(new HttpRequestProvider());
        public static ushort Port { get; set; } = 8701;

        public static void Start()
        {
            Server.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Loopback, Port)));

            Server.Use(new ExceptionHandler());
            Server.Use(new CompressionHandler(DeflateCompressor.Default, GZipCompressor.Default));
            Server.Use(new HttpRouter().With(string.Empty, new RestHandler<ServerInfo>(new ServerInfoRestController(), JsonResponseProvider.Default)));

            Server.Start();
        }
    }
}
