using LunaClient.Base;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoEvents : SubSystem<VesselProtoSystem>
    {
        /// <summary>
        /// This event is called after game is loaded, vessels initialized and all systems running and ready to fly
        /// </summary>
        public void OnFlightReady()
        {
            if (FlightGlobals.ActiveVessel != null && System.CheckVessel())
            {
                System.VesselReady = true;
                System.CurrentVesselSent = false;
            }
        }

        /// <summary>
        /// Called when the vessel has been modified (explosion, decouplers, etc)
        /// </summary>
        public void OnVesselWasModified(Vessel data)
        {
            //Vessel has been modified so send the new vessel
            System.CurrentVesselSent = false;
        }
    }
}