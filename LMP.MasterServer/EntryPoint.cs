using LMP.MasterServer.Http;
using LunaCommon;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleLogger = LunaCommon.ConsoleLogger;
using LogLevels = LunaCommon.LogLevels;

namespace LMP.MasterServer
{
    /// <summary>
    /// This program is the one who does the punchtrough between a nat client and a nat server. 
    /// You should only run if you agree in the forum to do so and your server ip is listed in:
    /// https://raw.githubusercontent.com/LunaMultiplayer/LunaMultiplayer/master/MasterServersList
    /// </summary>
    public static class EntryPoint
    {
        public static void Stop()
        {
            Lidgren.MasterServer.RunServer = false;
            LunaHttpServer.Server.Dispose();
        }

        public static void MainEntryPoint(string[] args)
        {
            if (Common.PlatformIsWindows())
                ConsoleUtil.DisableConsoleQuickEdit();

            Console.Title = $"LMP MasterServer {LmpVersioning.CurrentVersion}";

            if (args.Any(a => a.Contains("nightly")))
                Console.Title += " NIGHTLY";

            Console.OutputEncoding = Encoding.Unicode;

            var commandLineArguments = new Arguments(args);
            if (commandLineArguments["h"] != null)
            {
                ShowCommandLineHelp();
                return;
            }

            if (!ParseMasterServerPortNumber(commandLineArguments)) return;
            if (!ParseHttpServerPort(commandLineArguments)) return;

            ConsoleLogger.Log(LogLevels.Normal, $"Starting MasterServer at port: {Lidgren.MasterServer.Port}");
            ConsoleLogger.Log(LogLevels.Normal, $"Listening for GET requests at port: {LunaHttpServer.Port}");

            if (CheckPort())
            {
                Lidgren.MasterServer.RunServer = true;
                LunaHttpServer.Start();
                Task.Run(() => Lidgren.MasterServer.Start());
            }
        }

        private static bool CheckPort()
        {
            if (Common.PortIsInUse(Lidgren.MasterServer.Port))
            {
                ConsoleLogger.Log(LogLevels.Error, $"Port {Lidgren.MasterServer.Port} is already in use!");
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

            ConsoleLogger.Log(LogLevels.Error, $"Invalid port specified: {commandLineArguments[parameter].Trim()}");
            return false;
        }

        #endregion
    }
}
