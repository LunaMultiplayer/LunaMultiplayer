using LunaClient.Base;
using UniLinq;

namespace LunaClient.Systems.VesselImmortalSys
{
    public class VesselImmortalEvents : SubSystem<VesselImmortalSystem>
    {
        /// <summary>
        /// Set vessel immortal state just when the vessel loads
        /// </summary>
        public void VesselLoaded(Vessel vessel)
        {
            if(vessel == null) return;

            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.id == vessel.id)
                return;
            if (System.OwnedVessels.Any(v => v?.id == vessel.id))
                return;

            System.SetVesselImmortalState(vessel, true);
        }
    }
}
