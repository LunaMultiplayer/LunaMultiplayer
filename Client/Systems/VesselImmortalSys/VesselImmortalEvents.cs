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
            if(System.OtherPeopleVessels.Any(v=> v.id == vessel.id))
                System.SetVesselImmortalState(vessel, true);
        }
    }
}
