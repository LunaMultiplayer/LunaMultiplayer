using Lidgren.Network;
using LunaCommon.Message;
using Server.Client;
using Server.Settings;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Server.Context
{
    public static class ServerContext
    {
        public static ConcurrentDictionary<IPEndPoint, ClientStructure> Clients { get; set; } =
            new ConcurrentDictionary<IPEndPoint, ClientStructure>();

        public static bool ServerRunning { get; set; }
        public static bool ServerStarting { get; set; }
        public static Stopwatch ServerClock { get; } = new Stopwatch();
        public static long StartTime { get; set; }
        public static string ModFilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LMPModControl.xml");
        public static int PlayerCount => ClientRetriever.GetActiveClientCount();
        public static string UniverseDirectory { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Universe");
        public static string ConfigDirectory { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
        public static string Players => ClientRetriever.GetActivePlayerNames();
        public static int Day { get; set; }
        public static long LastPlayerActivity { get; set; }
        public static bool UsePassword => !string.IsNullOrEmpty(GeneralSettings.SettingsStore.Password);

        // Configuration object
        public static NetPeerConfiguration Config { get; } = new NetPeerConfiguration("LMP")
        {            
            SendBufferSize = 1500000, //500kb
            ReceiveBufferSize = 1500000, //500kb
            DefaultOutgoingMessageCapacity = 500000, //500kb
            SuppressUnreliableUnorderedAcks = true,
        };
        
        public static MasterServerMessageFactory MasterServerMessageFactory { get; } = new MasterServerMessageFactory();
        public static ServerMessageFactory ServerMessageFactory { get; } = new ServerMessageFactory();
        public static ClientMessageFactory ClientMessageFactory { get; } = new ClientMessageFactory();

        public static async void Shutdown()
        {
            await Task.Delay(1000);
            ServerStarting = false;
            ServerRunning = false;
        }
    }
}