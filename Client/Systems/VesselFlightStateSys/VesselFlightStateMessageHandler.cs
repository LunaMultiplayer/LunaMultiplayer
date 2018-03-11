using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateMessageHandler : SubSystem<VesselFlightStateSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselFlightStateMsgData msgData)) return;

            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;

            //Vessel might exist in the store but not in game (if the vessel is in safety bubble for example)
            VesselsProtoStore.UpdateVesselProtoFlightState(msgData);

            if (!System.FlightStatesDictionary.ContainsKey(msgData.VesselId))
            {
                System.VesselsToAssign.Enqueue(msgData.VesselId);
            }
            System.FlightStatesDictionary.GetOrAdd(msgData.VesselId, new VesselFlightStateUpdate()).SetTarget(msgData);
        }
    }
}
