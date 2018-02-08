using LunaClient.Base;
using LunaClient.VesselUtilities;

namespace LunaClient.Systems.VesselPrecalcSys
{
    public class VesselPrecalcEvents : SubSystem<VesselPrecalcSystem>
    {
        public void OnVesselPrecalcAssign(Vessel data)
        {
            data.precalc = data.gameObject.AddComponent<LunaPrecalc>();
        }
    }
}
