using LunaClient.Base;

namespace LunaClient.Systems.VesselRangeSys
{
    public class VesselRangeEvents : SubSystem<VesselRangeSystem>
    {
        public void OnVesselAwakeSetDefaultRange(Vessel data)
        {
            data.vesselRanges = System.LmpRanges;
        }
    }
}
