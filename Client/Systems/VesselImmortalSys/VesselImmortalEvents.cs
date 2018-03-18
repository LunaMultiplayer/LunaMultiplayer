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

            //Do it this way as vessel can become null later one
            var vesselId = vessel.id;
            if (System.OtherPeopleVessels.Any(v => v.id == vesselId))
            {
                System.SetVesselImmortalState(vessel, true);
            }
        }
    }
}
