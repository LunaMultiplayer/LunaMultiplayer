using LunaClient.Base;

namespace LunaClient.Systems.VesselPrecalcSys
{
    public class VesselPrecalcEvents : SubSystem<VesselPrecalcSystem>
    {
        public void OnVesselPrecalcAssign(Vessel vessel)
        {
            vessel.precalc = vessel.gameObject.AddComponent<LunaPrecalc>();
            if (vessel.isEVA)
                vessel.vesselRanges = System.LmpEvaRanges;
        }

        public void VesselLoaded(Vessel vessel)
        {
            if (vessel.precalc?.GetType() != typeof(LunaPrecalc))
            {
                vessel.precalc = vessel.gameObject.AddComponent<LunaPrecalc>();
            }
        }
    }
}
