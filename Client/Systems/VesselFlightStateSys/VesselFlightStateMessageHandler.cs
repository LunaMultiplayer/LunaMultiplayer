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

            var vesselId = msgData.VesselId;
            if (!VesselCommon.DoVesselChecks(vesselId))
                return;

            //Vessel might exist in the store but not in game (while in KSC for example)
            VesselsProtoStore.UpdateVesselProtoFlightState(msgData);

            //System is not ready nor in use so just skip the message
            if (!System.FlightStateSystemReady)
                return;

            //We are not close (unpacked range) to this vessel so ignore the message
            if (!System.FlyByWireDictionary.ContainsKey(vesselId))
                return;

            if (VesselFlightStateSystem.CurrentFlightState.TryGetValue(vesselId, out var currentFlightState) && currentFlightState.GameTimeStamp > msgData.GameTime)
            {
                //A user reverted, so clear his message queue and start from scratch
                System.RemoveVesselFromSystem(vesselId);
            }

            if (!VesselFlightStateSystem.CurrentFlightState.ContainsKey(vesselId))
            {
                VesselFlightStateSystem.CurrentFlightState.TryAdd(vesselId, new VesselFlightStateUpdate(msgData));
                VesselFlightStateSystem.TargetFlightStateQueue.TryAdd(vesselId, new FlightStateQueue());
            }
            else
            {
                VesselFlightStateSystem.TargetFlightStateQueue[vesselId].Enqueue(msgData);
            }
        }
    }
}
