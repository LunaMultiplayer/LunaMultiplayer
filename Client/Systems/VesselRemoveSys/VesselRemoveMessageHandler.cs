using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageHandler : SubSystem<VesselRemoveSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselRemoveMsgData msgData)) return;

            if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel?.id == msgData.VesselId)
                return;

            if (msgData.AddToKillList)
                System.AddToKillList(msgData.VesselPersistentId, msgData.VesselId, "Received a vessel remove message from server");

            //Do a simple kill and accept future updates of that vessel instead of just ignoring them
            if (!msgData.AddToKillList)
                System.KillVessel(msgData.VesselPersistentId, msgData.VesselId, "Received a fast vessel kill message from server");
        }
    }
}
