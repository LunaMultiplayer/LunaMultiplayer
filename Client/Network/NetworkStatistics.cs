using LunaCommon.Message.Base;
using System;
using LunaCommon;

namespace LunaClient.Network
{
    public class NetworkStatistics
    {
        public static float PingMs { get; set; }
        public static DateTime LastReceiveTime { get; set; }
        public static DateTime LastSendTime { get; set; }

        public static long TimeOffset => TimeSpan.FromSeconds(NetworkMain.ClientConnection?.ServerConnection?.RemoteTimeOffset ?? 0).Ticks;

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
                    return TimeOffset;
                case "LastSendTime":
                    return (long)(LunaTime.Now - LastSendTime).TotalMilliseconds;
                case "LastReceiveTime":
                    return (long)(LunaTime.Now - LastReceiveTime).TotalMilliseconds;
                case "MessagesInCache":
                    return MessageStore.GetMessageCount(null);
                case "MessageDataInCache":
                    return MessageStore.GetMessageDataCount(null);
            }
            return 0;
        }
    }
}
