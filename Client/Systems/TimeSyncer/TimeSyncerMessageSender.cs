using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.TimeSyncer
{
    public class TimeSyncerMessageSender : SubSystem<TimeSyncerSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSystem.Singleton.QueueOutgoingMessage(MessageFactory.CreateNew<SyncTimeCliMsg>(msg));
        }

        public void SendTimeSyncRequest()
        {
            //This message will be rewrited JUST before serialization so the time is more precise
            SendMessage(new SyncTimeRequestMsgData()); 
        }
    }
}