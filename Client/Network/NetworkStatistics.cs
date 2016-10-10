using System;

namespace LunaClient.Network
{
    public class NetworkStatistics
    {
        public static double PingMs { get; set; }
        public static long LastReceiveTime { get; set; }
        public static long LastSendTime { get; set; }

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
                case "LastSendTime":
                    return (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - LastSendTime);
                case "LastReceiveTime":
                    return (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - LastReceiveTime);
                case "QueuedOutgoingMessages":
                    return NetworkSender.OutgoingMessages.Count;
            }
            return 0;
        }
    }
}
