using LmpClient.Base;
using LmpCommon.Message.Data.Vessel;

namespace LmpClient.Systems.VesselDecoupleSys
{
    public class VesselDecoupleQueue : CachedConcurrentQueue<VesselDecouple, VesselDecoupleMsgData>
    {
        protected override void AssignFromMessage(VesselDecouple value, VesselDecoupleMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;

            value.PartFlightId = msgData.PartFlightId;
            value.BreakForce = msgData.BreakForce;

            value.NewVesselId = msgData.NewVesselId;
        }
    }
}
