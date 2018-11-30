using LmpClient.Base;
using LmpCommon.Message.Data.Vessel;

namespace LmpClient.Systems.VesselUndockSys
{
    public class VesselUndockQueue : CachedConcurrentQueue<VesselUndock, VesselUndockMsgData>
    {
        protected override void AssignFromMessage(VesselUndock value, VesselUndockMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;

            value.PartFlightId = msgData.PartFlightId;
            value.NewVesselId = msgData.NewVesselId;

            value.DockedInfo = new DockedVesselInfo
            {
                name = msgData.DockedInfoName.Clone() as string,
                rootPartUId = msgData.DockedInfoRootPartUId,
                vesselType = (VesselType)msgData.DockedInfoVesselType
            };
        }
    }
}
