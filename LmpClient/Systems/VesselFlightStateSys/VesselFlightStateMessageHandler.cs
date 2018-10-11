using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselFlightStateSys
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

            //System is not ready nor in use so just skip the message
            if (!System.FlightStateSystemReady)
                return;

            //We are not close (unpacked range) to this vessel so ignore the message
            if (!System.FlyByWireDictionary.ContainsKey(vesselId))
                return;

            if (!VesselFlightStateSystem.CurrentFlightState.ContainsKey(vesselId))
            {
                VesselFlightStateSystem.CurrentFlightState.TryAdd(vesselId, new VesselFlightStateUpdate(msgData));
                VesselFlightStateSystem.TargetFlightStateQueue.TryAdd(vesselId, new FlightStateQueue());
            }
            else
            {
                VesselFlightStateSystem.TargetFlightStateQueue.TryGetValue(vesselId, out var queue);
                queue?.Enqueue(msgData);
            }
        }
    }
}
