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
    [HarmonyPatch("TrackRigidbody")]
    public class OrbitDriver_TrackRigidbody
    {
        /// <summary>
        /// For the prefix we set the vessel as NON kinematic if it's controlled / updated by another player so the TrackRigidbody works as expected
        /// </summary>
        [HarmonyPrefix]
        private static bool PrefixTrackRigidbody(OrbitDriver __instance, CelestialBody refBody, double fdtOffset, ref double ___updateUT)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;
            if (__instance.vessel == null) return true;

            TrackRigidbody(__instance, refBody, fdtOffset, ref ___updateUT);

            return false;
        }

        private static void TrackRigidbody(OrbitDriver driver, CelestialBody refBody, double fdtOffset, ref double updateUT)
        {
            updateUT = Planetarium.GetUniversalTime();
            if (driver.vessel != null)
            {
                driver.pos = (driver.vessel.CoMD - driver.referenceBody.position).xzy;
            }
            if (driver.vessel != null && driver.vessel.rootPart != null && driver.vessel.rootPart.rb != null) //  && !driver.vessel.rootPart.rb.isKinematic
            {
                updateUT += fdtOffset;
                driver.vel = driver.vessel.velocityD.xzy + driver.orbit.GetRotFrameVelAtPos(driver.referenceBody, driver.pos);
            }
            else if (driver.updateMode == OrbitDriver.UpdateMode.IDLE)
            {
                driver.vel = driver.orbit.GetRotFrameVel(driver.referenceBody);
            }
            if (refBody != driver.referenceBody)
            {
                if (driver.vessel != null)
                {
                    driver.pos = (driver.vessel.CoMD - refBody.position).xzy;
                }
                driver.vel = driver.vel + (driver.referenceBody.GetFrameVel() - refBody.GetFrameVel());
            }
            driver.lastTrackUT = updateUT;

            if (VesselPositionSystem.Singleton.VesselHavePositionUpdatesQueued(driver.vessel.id))
            {
                //Do not call UpdateFromStateVectors as that would update the orbit data BASED on the current vessel position
                return;
            }
            else
            {
                driver.orbit.UpdateFromStateVectors(driver.pos, driver.vel, refBody, updateUT);
                driver.pos.Swizzle();
                driver.vel.Swizzle();
            }
        }
    }
}
