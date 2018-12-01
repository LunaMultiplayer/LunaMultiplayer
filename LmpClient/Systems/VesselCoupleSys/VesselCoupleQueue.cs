using LmpClient.Base;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Vessel;

namespace LmpClient.Systems.VesselCoupleSys
{
    public class VesselCoupleQueue : CachedConcurrentQueue<VesselCouple, VesselCoupleMsgData>
    {
        protected override void AssignFromMessage(VesselCouple value, VesselCoupleMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;

            value.PartFlightId = msgData.PartFlightId;
            value.CoupledPartFlightId = msgData.CoupledPartFlightId;

            value.CoupledVesselId = msgData.CoupledVesselId;

            value.Trigger = (CoupleTrigger)msgData.Trigger;
        }
    }
}
