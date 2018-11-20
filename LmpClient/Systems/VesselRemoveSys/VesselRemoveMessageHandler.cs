using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageHandler : SubSystem<VesselRemoveSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselRemoveMsgData msgData)) return;

            if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == msgData.VesselId)
                return;

            System.KillVessel(msgData.VesselId, msgData.AddToKillList, "Received a vessel remove message from server");
        }
    }
}
