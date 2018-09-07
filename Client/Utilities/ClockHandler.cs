using System.Collections.Generic;
using System.Linq;

namespace LunaClient.Utilities
{
    /// <summary>
    /// Class that handles all clock related things of KSP
    /// </summary>
    public class ClockHandler
    {
        private static readonly List<Vessel> VesselsToPack = new List<Vessel>();

        /// <summary>
        /// Call this method to set a new time using the planetarium
        /// </summary>
        public static void StepClock(double targetTick)
        {
            if (HighLogic.LoadedScene == GameScenes.LOADING)
            {
                LunaLog.Log("[LMP] Skipping StepClock in loading screen");
                return;
            }
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
            //TODO: Check if this is really needed...
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

            if (FlightGlobals.Vessels != null)
            {
                VesselsToPack.Clear();
                VesselsToPack.AddRange(FlightGlobals.Vessels.Where(v => v != null && !v.packed));
                foreach (var vessel in VesselsToPack)
                {
                    try
                    {
                        if (FlightGlobals.ActiveVessel.id != vessel.id || SafeToStepClock(vessel, targetTick))
                            vessel.GoOnRails();
                    }
                    catch
                    {
                        LunaLog.LogError($"[LMP] Error packing vessel {vessel?.id}");
                    }
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
                    return true;
                case Vessel.Situations.SUB_ORBITAL:
                    var altitudeAtUt = checkVessel.orbit.getRelativePositionAtUT(targetTick).magnitude;
                    return altitudeAtUt > checkVessel.mainBody.Radius + 10000 && checkVessel.altitude > 10000;
                default:
                    return false;
            }
        }
    }
}
