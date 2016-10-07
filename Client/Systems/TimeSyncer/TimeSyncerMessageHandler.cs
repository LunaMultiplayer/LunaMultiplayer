using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.TimeSyncer
{
    public class TimeSyncerMessageHandler : SubSystem<TimeSyncerSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as SyncTimeReplyMsgData;
            if (msgData == null) return;

            System.ServerStartTime = msgData.ServerStartTime;
            System.HandleSyncTime(messageData.ReceiveTime, msgData.ClientSendTime, msgData.ServerReceiveTime, msgData.ServerSendTime);
        }
    }
}