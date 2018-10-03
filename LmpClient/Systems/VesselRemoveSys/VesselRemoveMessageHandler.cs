using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselRemoveSys
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

            //Do a simple kill and accept future updates of that vessel instead of just ignoring them
            if (!msgData.AddToKillList)
                System.KillVessel(msgData.VesselId, "Received a fast vessel kill message from server");
        }
    }
}
