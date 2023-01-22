using LmpCommon;
using LmpMasterServer.Dedicated;
using LmpMasterServer.Http;
using LmpMasterServer.Log;
using LmpMasterServer.Upnp;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmpMasterServer
{
    /// <summary>
    /// This program is the one who does the punchtrough between a nat client and a nat server.
    /// You should only run if you agree in the forum to do so and your server ip is listed in:
    /// https://github.com/LunaMultiplayer/LunaMultiplayer/raw/master/MasterServersList/MasterServersList.txt
    /// </summary>
    public static class EntryPoint
    {
        private static bool IsNightly { get; set; }

        public static void Stop()
        {
            Lidgren.MasterServer.RunServer = false;
            LunaHttpServer.Server.Dispose();
            MasterServerPortMapper.RemoveOpenedPorts().Wait();
        }

        public static void MainEntryPoint(string[] args)
        {
            MasterServerPortMapper.UseUpnp = !args.Any(a => a.Contains("noupnp"));
            IsNightly = args.Any(a => a.Contains("nightly"));
            if (Common.PlatformIsWindows())
            {
                ConsoleUtil.DisableConsoleQuickEdit();

                Console.Title = $"LMP MasterServer {LmpVersioning.CurrentVersion}";

                if (IsNightly)
                    Console.Title += " NIGHTLY";
            }

            Console.OutputEncoding = Encoding.UTF8;

            var commandLineArguments = new Arguments(args);
            if (commandLineArguments["h"] != null)
            {
                ShowCommandLineHelp();
                return;
            }

            if (!ParseMasterServerPortNumber(commandLineArguments)) return;
            if (!ParseHttpServerPort(commandLineArguments)) return;
            MasterServerPortMapper.OpenPort().GetAwaiter().GetResult();

            LunaLog.Normal($"Starting MasterServer at port: {Lidgren.MasterServer.Port}");
            if (IsNightly)
                LunaLog.Normal("Will download NIGHTLY versions!");
            LunaLog.Normal($"Listening for GET requests at port: {LunaHttpServer.Port}");

            if (CheckPort())
            {
                Lidgren.MasterServer.RunServer = true;
                Http.Handlers.WebHandler.InitWebFiles();
                LunaHttpServer.Start();
                Task.Run(DedicatedServerRetriever.RefreshDedicatedServersList);
                Task.Run(MasterServerPortMapper.RefreshUpnpPort);
                Task.Run(Lidgren.MasterServer.Start);
            }
        }

        private static bool CheckPort()
        {
            if (LunaNetUtils.IsUdpPortInUse(Lidgren.MasterServer.Port))
            {
                LunaLog.Fatal($"Port {Lidgren.MasterServer.Port} is already in use!");
                return false;
            }
            return true;
        }

        private static void ShowCommandLineHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("LMP Master server");
            Console.WriteLine("This program is only used to introduce client and standard LMP servers.");
            Console.WriteLine("Check the wiki for details about running a master server.");
            Console.WriteLine("In order to run this program you need to open the port in your router.");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("/h                       ... Show this help");
            Console.WriteLine("/p:<port>                ... Start with the specified port (default port is 8700)");
            Console.WriteLine("/g:<port>                ... Reply to get petitions on the specified port (default is 8701)");
            Console.WriteLine("/nightly                 ... Keep this master server updated with last nightly version");
            Console.WriteLine("/noupdatecheck           ... Disable automatic updates");
            Console.WriteLine("/noupnp                  ... Disable upnp port forwarding");
            Console.WriteLine("");
        }

        #region Command line arguments parsing

        private static bool ParseHttpServerPort(Arguments commandLineArguments)
        {
            if (commandLineArguments["g"] != null)
            {
                if (!ParsePortNumber(commandLineArguments, "g", out var port))
                    return false;

                LunaHttpServer.Port = port;
            }
            return true;
        }

        private static bool ParseMasterServerPortNumber(Arguments commandLineArguments)
        {
            if (commandLineArguments["p"] != null)
            {
                if (!ParsePortNumber(commandLineArguments, "p", out var port))
                    return false;

                Lidgren.MasterServer.Port = port;
            }
            return true;
        }

        private static bool ParsePortNumber(Arguments commandLineArguments, string parameter, out ushort portNum)
        {
            if (ushort.TryParse(commandLineArguments[parameter].Trim(), out portNum))
                return true;

            LunaLog.Error($"Invalid port specified: {commandLineArguments[parameter].Trim()}");
            return false;
        }

        #endregion
    }
}
