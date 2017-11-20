using System.Collections.Generic;
using System.Linq;

namespace LunaClient.Utilities
{
    /// <summary>
    /// Class that handles all clock related things of KSP
    /// </summary>
    public class ClockHandler
    {
        private static List<Vessel> VesselsToPack = new List<Vessel>();

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

                try
                {
                    OrbitPhysicsManager.HoldVesselUnpack(5);
                }
                catch
                {
                    LunaLog.LogError("[LMP] Failed to hold vessel unpack");
                    return;
                }

                PutVesselsInPhysicsRangeOnRails(targetTick);
            }
            Planetarium.SetUniversalTime(targetTick);
        }
        
        private static void PutVesselsInPhysicsRangeOnRails(double targetTick)
        {
            if (FlightGlobals.Vessels != null)
            {
                VesselsToPack.Clear();
                VesselsToPack.AddRange(FlightGlobals.Vessels.Where(v => !v.packed && SafeToStepClock(v, targetTick)));
                foreach (var vessel in VesselsToPack)
                {
                    try
                    {
                        vessel?.GoOnRails();
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
