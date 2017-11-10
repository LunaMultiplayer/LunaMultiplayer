using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.TimeSyncer
{
    public class TimeSyncerMessageHandler : SubSystem<TimeSyncerSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is SyncTimeReplyMsgData msgData)) return;

            System.ServerStartTime = msgData.ServerStartTime;
            System.HandleSyncTime(messageData.ReceiveTime, msgData.ClientSendTime, msgData.ServerReceiveTime, msgData.ServerSendTime);
        }
    }
}