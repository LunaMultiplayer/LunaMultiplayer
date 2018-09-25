using LmpClient.Base;
using LmpCommon.Message.Data.Vessel;

namespace LmpClient.Systems.VesselActionGroupSys
{
    public class VesselActionGroupQueue : CachedConcurrentQueue<VesselActionGroup, VesselActionGroupMsgData>
    {
        protected override void AssignFromMessage(VesselActionGroup value, VesselActionGroupMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;
            value.ActionGroup = (KSPActionGroup)msgData.ActionGroup;
            value.Value = msgData.Value;
        }
    }
}
