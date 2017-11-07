using System;

namespace LunaClient.Network
{
    public class NetworkStatistics
    {
        public static float PingMs { get; set; }
        public static DateTime LastReceiveTime { get; set; }
        public static DateTime LastSendTime { get; set; }

        public static long GetStatistics(string statType)
        {
            switch (statType)
            {
                case "Ping":
                    return (long)PingMs;
                case "SentBytes":
                    //Build lidgren in DEBUG mode or: https://github.com/lidgren/lidgren-network-gen3/wiki/Statistics
                    return NetworkMain.ClientConnection.Statistics.SentBytes;
                case "ReceivedBytes":
                    //Build lidgren in DEBUG mode or: https://github.com/lidgren/lidgren-network-gen3/wiki/Statistics
                    return NetworkMain.ClientConnection.Statistics.ReceivedBytes;
                case "LastSendTime":
                    return (long)(DateTime.Now - LastSendTime).TotalMilliseconds;
                case "LastReceiveTime":
                    return (long)(DateTime.Now - LastReceiveTime).TotalMilliseconds;
            }
            return 0;
        }
    }
}
