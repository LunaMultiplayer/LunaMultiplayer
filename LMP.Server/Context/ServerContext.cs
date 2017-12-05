using Lidgren.Network;
using LMP.Server.Client;
using LMP.Server.Lidgren;
using LMP.Server.Settings;
using LunaCommon.Message;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace LMP.Server.Context
{
    public static class ServerContext
    {
        public static ConcurrentDictionary<IPEndPoint, ClientStructure> Clients { get; set; } =
            new ConcurrentDictionary<IPEndPoint, ClientStructure>();

        public static bool ServerRunning { get; set; }
        public static bool ServerStarting { get; set; }
        public static Stopwatch ServerClock { get; } = new Stopwatch();
        public static long StartTime { get; set; }
        public static string ModFilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LMPModControl.txt");
        public static int PlayerCount => ClientRetriever.GetActiveClientCount();
        public static string UniverseDirectory { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Universe");
        public static string ConfigDirectory { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
        public static string Players { get; } = ClientRetriever.GetActivePlayerNames();
        public static int Day { get; set; }
        public static long LastPlayerActivity { get; set; }

        // Configuration object
        public static NetPeerConfiguration Config { get; } = new NetPeerConfiguration("LMP")
        {
            AutoFlushSendQueue = false,
            SendBufferSize = 500000, //500kb
            ReceiveBufferSize = 500000, //500kb
            //Set it to false so lidgren doesn't wait until msg.size = MTU for sending
            Port = GeneralSettings.SettingsStore.Port,
            MaximumConnections = GeneralSettings.SettingsStore.MaxPlayers,
            SuppressUnreliableUnorderedAcks = true,
            PingInterval = GeneralSettings.SettingsStore.HearbeatMsInterval,
            ConnectionTimeout = GeneralSettings.SettingsStore.ConnectionMsTimeout
        };

        public static LidgrenServer LidgrenServer { get; } = new LidgrenServer();
        public static MasterServerMessageFactory MasterServerMessageFactory { get; } = new MasterServerMessageFactory();

        public static ServerMessageFactory ServerMessageFactory { get; } = new ServerMessageFactory();
        public static ClientMessageFactory ClientMessageFactory { get; } = new ClientMessageFactory();
    }
}