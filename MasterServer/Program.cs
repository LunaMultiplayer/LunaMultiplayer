using LunaCommon;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                Thread.Sleep(5000);
                return;
            }
            if (commandLineArguments["p"] != null)
            {
                MasterServer.Port = ushort.Parse(commandLineArguments["p"].Trim());
                Logger.Log(LogLevels.Normal, $"Starting MasterServer with port: {MasterServer.Port}");
            }
            else
            {
                MasterServer.Port = 6005;
                Logger.Log(LogLevels.Normal, "Starting MasterServer with default port: 6005");
            }

            MasterServer.Start();
        }
        
        private static void ShowCommandLineHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("LMP Master server");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("/h                       ... Show this help");
            Console.WriteLine("/p:<port>                ... Start with the specified port");
            Console.WriteLine("");
        }
    }
}
