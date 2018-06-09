using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class FlightStateQueue : CachedConcurrentQueue<VesselFlightStateUpdate, VesselFlightStateMsgData>
    {
        protected override void AssignFromMessage(VesselFlightStateUpdate value, VesselFlightStateMsgData msgData)
        {
            value.VesselId = msgData.VesselId;
            value.GameTimeStamp = msgData.GameTime;
            value.SubspaceId = msgData.SubspaceId;

            value.CtrlState.CopyFrom(msgData);
        }
    }
}
