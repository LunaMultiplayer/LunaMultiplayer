using HarmonyLib;
using LmpClient.Extensions;
using LmpClient.Systems.VesselPositionSys;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to do our own orbit calculations
    /// First we always call the updateFromParameters so the orbit information of every vessel is updated and then they are positioned correctly
    /// After that, we call the TrackRigidbody if needed but we NEVER update the orbital parameters based on the vessel position
    /// </summary>
    [HarmonyPatch(typeof(OrbitDriver))]
    [HarmonyPatch("UpdateOrbit")]
    public class OrbitDriver_UpdateOrbit
    {
        /// <summary>
        /// For the prefix we set the vessel as NON kinematic if it's controlled / updated by another player so the TrackRigidbody works as expected
        /// </summary>
        [HarmonyPrefix]
        private static bool PrefixUpdateOrbit(OrbitDriver __instance, bool offset, ref bool ___ready, ref double ___fdtLast, ref bool ___isHyperbolic)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;
            if (__instance.vessel == null) return true;
            if (FlightGlobals.ActiveVessel && __instance.vessel == FlightGlobals.ActiveVessel && __instance.vessel.packed) return true;
            // For packed vessels that LMP is actively managing, use our custom update to suppress spurious
            // on-rails SOI transitions. Stock UpdateOrbit detects SOI crossings from orbit parameters that
            // LMP resets every frame via UpdateFromStateVectors, causing repeated transition log spam.
            // The reference body for these vessels is controlled by Orbit[7] in position messages.
            if (!__instance.vessel.IsImmortal() && __instance.vessel.packed &&
                !VesselPositionSystem.Singleton.VesselHavePositionUpdatesQueued(__instance.vessel.id)) return true;

            UpdateOrbit(__instance, offset, ref ___ready, ref ___fdtLast, ref ___isHyperbolic);

            return false;
        }

        private static void UpdateOrbit(OrbitDriver driver, bool offset, ref bool ready, ref double fdtLast, ref bool isHyperbolic)
        {
            if (!ready) return;
            driver.lastMode = driver.updateMode;

            var hasQueuedUpdates = VesselPositionSystem.Singleton.VesselHavePositionUpdatesQueued(driver.vessel.id);

            //Always call updateFromParameters so the vessel is positioned based on the orbital data
            if ((hasQueuedUpdates && driver.updateMode == OrbitDriver.UpdateMode.TRACK_Phys)
                || driver.updateMode == OrbitDriver.UpdateMode.UPDATE)
            {
                driver.updateFromParameters();
                // When LMP is actively syncing a vessel, the reference body is driven by the incoming
                // position message (Orbit[7]). Calling CheckDominantBody here would race against that,
                // spuriously switching the body and producing on-rails SOI transition spam in map view.
                if (driver.vessel && !hasQueuedUpdates)
                {
                    driver.CheckDominantBody(driver.referenceBody.position + driver.pos);
                }
            }

            if (driver.vessel && driver.vessel.rootPart && driver.vessel.rootPart.rb)
            {
                if (!offset)
                {
                    fdtLast = 0;
                }
                if (!driver.CheckDominantBody(driver.vessel.CoMD))
                {
                    driver.TrackRigidbody(driver.referenceBody, -fdtLast);
                }
            }

            fdtLast = (double)TimeWarp.fixedDeltaTime;
            if (isHyperbolic && driver.orbit.eccentricity < 1)
            {
                isHyperbolic = false;
                if (driver.vessel != null)
                {
                    GameEvents.onVesselOrbitClosed.Fire(driver.vessel);
                }
            }
            if (!isHyperbolic && driver.orbit.eccentricity > 1)
            {
                isHyperbolic = true;
                if (driver.vessel != null)
                {
                    GameEvents.onVesselOrbitEscaped.Fire(driver.vessel);
                }
            }
            if (driver.drawOrbit)
            {
                driver.orbit.DrawOrbit();
            }
        }
    }
}
