using LunaCommon;
using System;
using System.Text;

namespace MasterServer
{
    /// <summary>
    /// This program is the one who does the punchtrough between a nat client and a nat server. 
    /// You should only run if you agree in the forum to do so and your server ip is listed in:
    /// https://raw.githubusercontent.com/DaggerES/LunaMultiPlayer/master/MasterServersList
    /// </summary>
    static class Program
    {
        static void Main(string[] args)
        {
            Console.Title = $"LMPMasterServer {Common.CurrentVersion}";
            Console.OutputEncoding = Encoding.Unicode;

            var commandLineArguments = new Arguments(args);
            if (commandLineArguments["h"] != null)
            {
                ShowCommandLineHelp();
                return;
            }

            if (commandLineArguments["p"] != null)
            {
                if (ushort.TryParse(commandLineArguments["p"].Trim(), out var parsedPort))
                    MasterServer.Port = parsedPort;
                else
                {
                    Logger.Log(LogLevels.Error, $"Invalid port specified");
                    return;
                }
            }
            else
            {
                MasterServer.Port = 6005;
            }

            Logger.Log(LogLevels.Normal, $"Starting MasterServer at port: {MasterServer.Port}");

            if (CheckPort())
                MasterServer.Start();
        }

        private static bool CheckPort()
        {
            if (Common.PortIsInUse(MasterServer.Port))
            {
                Logger.Log(LogLevels.Error, $"Port {MasterServer.Port} is already in use!");
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
            Console.WriteLine("/p:<port>                ... Start with the specified port (default port is 6005)");
            Console.WriteLine("");
        }
    }
}
