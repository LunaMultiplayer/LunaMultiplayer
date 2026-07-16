using LmpCommon;
using Server.Context;
using Server.Events;
using Server.Log;
using Server.Settings.Structures;
using Server.Web.Handlers;
using Server.Web.Structures;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using uhttpsharp;
using uhttpsharp.Handlers;
using uhttpsharp.Handlers.Compression;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;

namespace Server.Web
{
    /// <summary>
    /// This class controls the information shown in JSON format when you go to http://yourip:8900
    /// </summary>
    public static class WebServer
    {
        private static readonly HttpServer Server = new HttpServer(new HttpRequestProvider());
        public static readonly ServerInformation ServerInformation = new ServerInformation();

        static WebServer() => ExitEvent.ServerClosing += StopWebServer;

        /// <summary>
        /// Starts the web server
        /// </summary>
        public static void StartWebServer()
        {
            if (WebsiteSettings.SettingsStore.EnableWebsite)
            {
                try
                {
                    if (!LunaNetUtils.IsTcpPortInUse(WebsiteSettings.SettingsStore.Port))
                    {
                        IPAddress listenAddress;
                        if (!string.IsNullOrEmpty(WebsiteSettings.SettingsStore.ListenAddress))
                        {
                            // Custom ListenAddress for web server, parse it
                            if (!IPAddress.TryParse(WebsiteSettings.SettingsStore.ListenAddress, out listenAddress))
                            {
                                // Parsing failed, fall back to IPAddress.Any
                                LunaLog.Warning(
                                    "Could not parse ListenAddress for web server, falling back to 0.0.0.0");
                            }
                        }
                        else
                        {
                            listenAddress = ServerContext.Config.LocalAddress;
                        }

                        var listener = new TcpListener(listenAddress, WebsiteSettings.SettingsStore.Port);
                        // Listen on dual-stack for the unspecified address in IPv6 format ([::]).
                        if (listenAddress.Equals(IPAddress.IPv6Any))
                        {
                            listener.Server.DualMode = true;
                        }

                        Server.Use(new TcpListenerAdapter(listener));
                        Server.Use(new ExceptionHandler());
                        Server.Use(new CompressionHandler(DeflateCompressor.Default, GZipCompressor.Default));
                        Server.Use(new HttpRouter().With(string.Empty, new RestHandler<ServerInformation>(new ServerInformationRestController(), JsonResponseProvider.Default)));
                        Server.Start();
                    }
                    else
                    {
                        LunaLog.Error("Could not start web server. Port is already in use.");
                        LunaLog.Info("You can change the web server settings inside 'Config/WebsiteSettings.xml'");
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Could not start web server. Details: {e}");
                }
            }
        }

        /// <summary>
        /// Starts the web server
        /// </summary>
        public static void StopWebServer()
        {
            if (WebsiteSettings.SettingsStore.EnableWebsite)
            {
                try
                {
                    Server.Dispose();
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Could not stop web server." + Environment.NewLine + $"Details: {e}");
                }
            }
        }

        /// <summary>
        /// Refresh the server information
        /// </summary>
        public static async Task RefreshWebServerInformationAsync()
        {
            if (WebsiteSettings.SettingsStore.EnableWebsite)
            {
                while (ServerContext.ServerRunning)
                {
                    ServerInformation.Refresh();
                    await Task.Delay((int)TimeSpan.FromMilliseconds(WebsiteSettings.SettingsStore.RefreshIntervalMs).TotalMilliseconds);
                }
            }
        }
    }
}
