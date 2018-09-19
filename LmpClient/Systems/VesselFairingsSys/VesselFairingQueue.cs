using LmpClient.Base;
using LmpCommon.Message.Data.Vessel;

namespace LmpClient.Systems.VesselFairingsSys
{
    public class VesselFairingQueue : CachedConcurrentQueue<VesselFairing, VesselFairingMsgData>
    {
        protected override void AssignFromMessage(VesselFairing value, VesselFairingMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;

            value.PartFlightId = msgData.PartFlightId;
            value.PartPersistentId = msgData.PartPersistentId;
        }
    }
}
