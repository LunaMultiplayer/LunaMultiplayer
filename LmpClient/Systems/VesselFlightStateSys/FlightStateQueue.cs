using LmpClient.Base;
using LmpClient.Extensions;
using LmpCommon.Message.Data.Vessel;

namespace LmpClient.Systems.VesselFlightStateSys
{
    public class FlightStateQueue : CachedConcurrentQueue<VesselFlightStateUpdate, VesselFlightStateMsgData>
    {
        protected override void AssignFromMessage(VesselFlightStateUpdate value, VesselFlightStateMsgData msgData)
        {
            value.VesselId = msgData.VesselId;
            value.GameTimeStamp = msgData.GameTime;
            value.SubspaceId = msgData.SubspaceId;
            value.PingMs = msgData.PingSec;

            value.CtrlState.CopyFrom(msgData);
        }
    }
}
