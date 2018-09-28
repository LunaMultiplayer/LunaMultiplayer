namespace LmpClient.Utilities
{
    /// <summary>
    /// Class that handles all clock related things of KSP
    /// </summary>
    public class ClockHandler
    {
        /// <summary>
        /// Call this method to set a new time using the planetarium
        /// </summary>
        public static void StepClock(double targetTick)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (FlightGlobals.fetch.activeVessel == null || !FlightGlobals.ready)
                {
                    LunaLog.Log("[LMP] Skipping StepClock (active vessel is null or not ready)");
                    return;
                }
                
                PutVesselsInPhysicsRangeOnRails(targetTick);
            }
            Planetarium.SetUniversalTime(targetTick);
        }
        
        private static void PutVesselsInPhysicsRangeOnRails(double targetTick)
        {
            /*  
             *  Dagger - why you made all vessels go on rails? v.GoOnRails();
             *  Darklight - Because the universe time is jumped
             *  Dagger - I just made some tests and I didn't noticed anything when doing Planetarium.SetUniversalTime(targetTick);
             *  and without making vessels on rails
             *  Darklight - Otherwise the posistion never updates and you freeze in your orbit for that jump
             *  Dagger - is that because your positioning is time dependant?
             *  Darklight - That jump should never be hit unless you are extreme lagging
             *  Dagger - yes, that's right but sometimes it happens
             *  Darklight - (or reverted), but any time the time is set, you should be on rails
             *  Darklight - Lets say you are orbital and are just passing the KSC, if you jump 5 minutes, you should be way past it, if you aren't on rails you'll still be over the KSC and you will fall behind packed vessels that are moving correctly
             */

            //Packing the vessels is needed. Otherwise in orbit if you lag, when syncing time the vessels will terribly far away
            if (FlightGlobals.Vessels != null)
            {
                foreach (var vessel in FlightGlobals.VesselsLoaded)
                {
                    if (vessel.packed || FlightGlobals.ActiveVessel == vessel) continue;
                    if (SafeToStepClock(vessel, targetTick))
                        vessel.GoOnRails();
                }
            }
        }

        private static bool SafeToStepClock(Vessel checkVessel, double targetTick)
        {
            if (checkVessel == null) return false;

            switch (checkVessel.situation)
            {
                case Vessel.Situations.LANDED:
                case Vessel.Situations.PRELAUNCH:
                case Vessel.Situations.SPLASHED:
                    return false; //If we Change the clock while landed/splashed the throttle goes back to 0
                case Vessel.Situations.ORBITING:
                case Vessel.Situations.ESCAPING:
                case Vessel.Situations.SUB_ORBITAL:
                    return true;
                default:
                    return false;
            }
        }
    }
}
