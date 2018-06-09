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
                System.AddToKillList(msgData.VesselId, "Received a vessel remove message from server");
            else //Do a simple kill and accept future updates of that vessel instead of just ignoring them
                System.KillVessel(msgData.VesselId, "Received a simple vessel kill message from server");
        }
    }
}
