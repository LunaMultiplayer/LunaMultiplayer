using LunaClient.Base;
using LunaClient.Base.Interface;
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

            if (System.FlightStatesDictionary.TryGetValue(msgData.VesselId, out var existingFlightState) && existingFlightState.EndTimeStamp < msgData.TimeStamp)
            {
                existingFlightState.SetTarget(msgData);
            }
        }
    }
}
