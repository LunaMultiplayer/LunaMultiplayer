using System.Linq;
using UnityEngine;

namespace LunaClient.Utilities
{
    /// <summary>
    /// Class that handles all clock related things of KSP
    /// </summary>
    public class ClockHandler
    {
        /// <summary>
        /// Call this method to set a new time
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
                try
                {
                    OrbitPhysicsManager.HoldVesselUnpack(5);
                }
                catch
                {
                    LunaLog.LogError("[LMP] Failed to hold vessel unpack");
                    return;
                }

                foreach (var vessel in FlightGlobals.VesselsLoaded.Where(v => !v.packed))
                {
                    if (vessel.isActiveVessel && SafeToStepClock(vessel, targetTick) ||
                        !vessel.isActiveVessel && vessel.situation != Vessel.Situations.PRELAUNCH)
                    {
                        try
                        {
                            //For prelaunch vessels, we should not go on rails as this will reset the throttles and such 
                            vessel.GoOnRails();
                        }
                        catch
                        {
                            LunaLog.LogError(vessel.isActiveVessel
                                ? $"[LMP] Error packing active vessel {vessel.id}"
                                : $"[LMP] Error packing vessel {vessel.id}");
                        }
                    }
                }
            }
            Planetarium.SetUniversalTime(targetTick);
        }

        private static bool SafeToStepClock(Vessel checkVessel, double targetTick)
        {
            switch (checkVessel.situation)
            {
                case Vessel.Situations.LANDED:
                case Vessel.Situations.PRELAUNCH:
                case Vessel.Situations.SPLASHED:
                    //TODO: Fix.  We need to be able to adjust the clock on the ground, but then it resets the throttle position and does physics easing.
                    //TODO: For now, disable stepping the clock while landed.
                    return checkVessel.srf_velocity.magnitude < 2;
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
