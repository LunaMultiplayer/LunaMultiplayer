using LmpCommon.Message.Base;

namespace LmpClient.Network
{
    public class NetworkStatistics
    {
        public static float PingSec => NetworkMain.ClientConnection.ServerConnection.AverageRoundtripTime;
        public static int SentBytes => NetworkMain.ClientConnection.Statistics.SentBytes;
        public static int ReceivedBytes => NetworkMain.ClientConnection.Statistics.ReceivedBytes;
        public static float TimeOffset => NetworkMain.ClientConnection?.ServerConnection?.RemoteTimeOffset ?? 0;
        public static int MessagesInCache => MessageStore.GetMessageCount(null);
        public static int MessageDataInCache => MessageStore.GetMessageDataCount(null);
    }
}
