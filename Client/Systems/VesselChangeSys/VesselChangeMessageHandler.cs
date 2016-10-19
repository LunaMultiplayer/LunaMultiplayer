using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselChangeSys
{
    public class VesselChangeMessageHandler : SubSystem<VesselChangeSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as VesselChangeMsgData;
            if (msgData == null) return;

            System.IncomingChanges.Enqueue(msgData);
        }
    }
}
