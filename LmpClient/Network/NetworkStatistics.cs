using LmpCommon.Message.Base;
using LmpCommon.Time;
using System;

namespace LmpClient.Network
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
                    return NetworkMain.ClientConnection.Statistics.SentBytes;
                case "ReceivedBytes":
                    return NetworkMain.ClientConnection.Statistics.ReceivedBytes;
                case "Latency":
                    return (long)NetworkMain.ClientConnection.ServerConnection.AverageRoundtripTime;
                case "TimeOffset":
                    return (long)TimeSpan.FromSeconds(NetworkMain.ClientConnection?.ServerConnection?.RemoteTimeOffset ?? 0).TotalMilliseconds;
                case "LastSendTime":
                    return (long)(LunaNetworkTime.UtcNow - LastSendTime).TotalMilliseconds;
                case "LastReceiveTime":
                    return (long)(LunaNetworkTime.UtcNow - LastReceiveTime).TotalMilliseconds;
                case "MessagesInCache":
                    return MessageStore.GetMessageCount(null);
                case "MessageDataInCache":
                    return MessageStore.GetMessageDataCount(null);
            }
            return 0;
        }
    }
}
