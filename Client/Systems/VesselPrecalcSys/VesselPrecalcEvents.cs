using LunaClient.Base;

namespace LunaClient.Systems.VesselPrecalcSys
{
    public class VesselPrecalcEvents : SubSystem<VesselPrecalcSystem>
    {
        public void OnVesselPrecalcAssign(Vessel vessel)
        {
            if (vessel.isEVA)
                vessel.vesselRanges = System.LmpEvaRanges;
        }
    }
}
