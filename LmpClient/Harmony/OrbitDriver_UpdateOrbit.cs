using Harmony;
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

            UpdateOrbit(__instance, offset, ref ___ready, ref ___fdtLast, ref ___isHyperbolic);

            return false;
        }

        private static void UpdateOrbit(OrbitDriver driver, bool offset, ref bool ready, ref double fdtLast, ref bool isHyperbolic)
        {
            if (!ready) return;
            driver.lastMode = driver.updateMode;

            //Always call updateFromParameters so the vessel is positioned based on the orbital data
            if ((VesselPositionSystem.Singleton.VesselHavePositionUpdatesQueued(driver.vessel.id) && driver.updateMode == OrbitDriver.UpdateMode.TRACK_Phys) 
                || driver.updateMode == OrbitDriver.UpdateMode.UPDATE)
            {
                driver.updateFromParameters();
                if (driver.vessel)
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
