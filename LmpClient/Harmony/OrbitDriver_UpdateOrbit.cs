using Harmony;
using LmpClient.Extensions;
using LmpCommon.Enums;
using UnityEngine;

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
        private static bool PrefixUpdateOrbit(OrbitDriver __instance, bool offset, ref bool ___ready, ref double ___fdtLast, ref bool ___isHyperbolic, ref double ___updateUT)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;
            if (__instance.vessel == null) return true;
            if (FlightGlobals.ActiveVessel == __instance.vessel) return true;
            if (!__instance.vessel.IsImmortal()) return true;

            UpdateOrbit(__instance, offset, ref ___ready, ref ___fdtLast, ref ___isHyperbolic, ref ___updateUT);

            return false;
        }

        private static void UpdateOrbit(OrbitDriver driver, bool offset, ref bool ready, ref double fdtLast, ref bool isHyperbolic, ref double updateUT)
        {
            if (!ready) return;
            driver.lastMode = driver.updateMode;

            //Always call updateFromParameters so the vessel is positioned based on the orbital data
            if (driver.updateMode == OrbitDriver.UpdateMode.TRACK_Phys || driver.updateMode == OrbitDriver.UpdateMode.UPDATE)
            {
                UpdateFromParameters(driver, ref updateUT);
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
                    TrackRigidbody(driver, driver.referenceBody, -fdtLast, ref updateUT);
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

            //Do not call UpdateFromStateVectors as that would update the orbit data BASED on the current vessel position
            //driver.orbit.UpdateFromStateVectors(driver.pos, driver.vel, refBody, updateUT);
            //driver.pos.Swizzle();
            //driver.vel.Swizzle();
        }

        private static void UpdateFromParameters(OrbitDriver driver, ref double updateUT)
        {
            updateUT = Planetarium.GetUniversalTime();
            driver.orbit.UpdateFromUT(updateUT);
            driver.pos = driver.orbit.pos;
            driver.vel = driver.orbit.vel;
            driver.pos.Swizzle();
            driver.vel.Swizzle();
            if (double.IsNaN(driver.pos.x))
            {
                MonoBehaviour.print(string.Concat(new object[] { "ObT : ", driver.orbit.ObT, "\nM : ", driver.orbit.meanAnomaly, "\nE : ", driver.orbit.eccentricAnomaly, "\nV : ", driver.orbit.trueAnomaly, "\nRadius: ", driver.orbit.radius, "\nvel: ", driver.vel.ToString(), "\nAN: ", driver.orbit.an.ToString(), "\nperiod: ", driver.orbit.period, "\n" }));
                if (driver.vessel)
                {
                    Debug.LogWarning(string.Concat("[OrbitDriver Warning!]: ", driver.vessel.vesselName, " had a NaN Orbit and was removed."));
                    driver.vessel.Unload();
                    Object.Destroy(driver.vessel.gameObject);
                    return;
                }
            }
            if (driver.reverse)
            {
                driver.referenceBody.position = (!driver.celestialBody ? (Vector3d)driver.driverTransform.position : driver.celestialBody.position) - driver.pos;
            }
            else if (driver.vessel)
            {
                //DO NOT update the vessel position here. The VesselPositioningSystem takes care of that
                //Vector3d vector3d = driver.driverTransform.rotation * driver.vessel.localCoM;
                //driver.vessel.SetPosition((driver.referenceBody.position + driver.pos) - vector3d);
            }
            else if (!driver.celestialBody)
            {
                driver.driverTransform.position = driver.referenceBody.position + driver.pos;
            }
            else
            {
                driver.celestialBody.position = driver.referenceBody.position + driver.pos;
            }
        }
    }
}
