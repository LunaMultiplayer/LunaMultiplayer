using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;

namespace LunaClient.Systems.VesselFairingsSys
{
    public class VesselFairingQueue : CachedConcurrentQueue<VesselFairing, VesselFairingMsgData>
    {
        protected override void AssignFromMessage(VesselFairing value, VesselFairingMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;

            value.PartFlightId = msgData.PartFlightId;
        }
    }
}
