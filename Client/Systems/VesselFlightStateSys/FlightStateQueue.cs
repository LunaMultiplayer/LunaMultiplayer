using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using System;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class FlightStateQueue : VesselCachedConcurrentQueue<VesselFlightStateUpdate, VesselFlightStateMsgData>
    {
        public FlightStateQueue(Guid vesselId) : base(vesselId)
        {
        }

        protected override bool CurrentDictionaryContainsKey(Guid vesselId) => VesselFlightStateSystem.CurrentFlightState.ContainsKey(vesselId);
        protected override int GetSubspaceIdFromValue(VesselFlightStateUpdate value) => value.SubspaceId;
        protected override double GetTimestampFromValue(VesselFlightStateUpdate value) => value.GameTimeStamp;

        protected override void AssignFromMessage(VesselFlightStateUpdate value, VesselFlightStateMsgData msgData)
        {
            value.VesselId = msgData.VesselId;
            value.GameTimeStamp = msgData.GameTime;
            value.SubspaceId = msgData.SubspaceId;

            value.CtrlState.CopyFrom(msgData);
        }
    }
}
