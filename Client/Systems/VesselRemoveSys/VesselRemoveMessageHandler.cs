using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageHandler : SubSystem<VesselRemoveSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is VesselRemoveMsgData msgData)) return;

            LunaLog.Log($"[LMP]: Received a vessel remove message. Removing vessel: {msgData.VesselId}");
            System.AddToKillList(msgData.VesselId);
        }
    }
}
