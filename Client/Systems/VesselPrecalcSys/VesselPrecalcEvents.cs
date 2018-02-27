using LunaClient.Base;

namespace LunaClient.Systems.VesselPrecalcSys
{
    public class VesselPrecalcEvents : SubSystem<VesselPrecalcSystem>
    {
        public void OnVesselPrecalcAssign(Vessel data)
        {
            data.precalc = data.gameObject.AddComponent<LunaPrecalc>();
        }

        public void VesselLoaded(Vessel data)
        {
            if (data.precalc?.GetType() != typeof(LunaPrecalc))
            {
                data.precalc = data.gameObject.AddComponent<LunaPrecalc>();
            }
        }
    }
}
