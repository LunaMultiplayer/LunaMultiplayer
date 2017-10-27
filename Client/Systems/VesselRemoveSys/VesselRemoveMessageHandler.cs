using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;
using UniLinq;

namespace LunaClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageHandler : SubSystem<VesselRemoveSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is VesselRemoveMsgData msgData)) return;

            var vessel = FlightGlobals.Vessels.FirstOrDefault(v => v.id == msgData.VesselId);
            if (vessel == null) return;

            LunaLog.Log($"[LMP]: Removing vessel: {msgData.VesselId}");
            System.AddToKillList(vessel, true);
        }
    }
}
