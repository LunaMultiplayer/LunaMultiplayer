using LmpCommon.Message.Base;
using LmpCommon.Time;
using System;

namespace LmpClient.Network
{
    public enum Statistic
    {
        Ping,
        SentBytes,
        ReceivedBytes,
        TimeOffset,
        LastSendTime,
        LastReceiveTime,
        MessagesInCache,
        MessageDataInCache
    }

    public class NetworkStatistics
    {
        public static DateTime LastReceiveTime { get; set; }
        public static DateTime LastSendTime { get; set; }

        public static float GetStatistics(Statistic statType)
        {
            switch (statType)
            {
                case Statistic.Ping:
                    return NetworkMain.ClientConnection.ServerConnection.AverageRoundtripTime;
                case Statistic.SentBytes:
                    return NetworkMain.ClientConnection.Statistics.SentBytes;
                case Statistic.ReceivedBytes:
                    return NetworkMain.ClientConnection.Statistics.ReceivedBytes;
                case Statistic.TimeOffset:
                    return (float)TimeSpan.FromSeconds(NetworkMain.ClientConnection?.ServerConnection?.RemoteTimeOffset ?? 0).TotalMilliseconds;
                case Statistic.LastSendTime:
                    return (float)(LunaNetworkTime.UtcNow - LastSendTime).TotalMilliseconds;
                case Statistic.LastReceiveTime:
                    return (float)(LunaNetworkTime.UtcNow - LastReceiveTime).TotalMilliseconds;
                case Statistic.MessagesInCache:
                    return MessageStore.GetMessageCount(null);
                case Statistic.MessageDataInCache:
                    return MessageStore.GetMessageDataCount(null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }
    }
}
