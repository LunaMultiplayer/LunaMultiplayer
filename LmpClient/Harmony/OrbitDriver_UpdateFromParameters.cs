using HarmonyLib;
using LmpClient.Systems.VesselPositionSys;
using LmpClient.Systems.VesselRemoveSys;
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
    [HarmonyPatch("updateFromParameters")]
    [HarmonyPatch(new[] { typeof(bool) })]
    public class OrbitDriver_UpdateFromParameters
    {
        /// <summary>
        /// We override this method to remove corrupt vessels from the server
        /// </summary>
        [HarmonyPrefix]
        private static bool PrefixUpdateFromParameters(OrbitDriver __instance, ref double ___updateUT)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;
            if (__instance.vessel == null) return true;

            UpdateFromParameters(__instance, ref ___updateUT);

            return false;
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
                    Debug.LogWarning(string.Concat("[LMP - OrbitDriver Warning!]: ", driver.vessel.vesselName, " had a NaN Orbit and was removed."));
                    driver.vessel.Unload();

                    VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(driver.vessel.id, true);
                    VesselRemoveSystem.Singleton.KillVessel(driver.vessel.id, true, "Corrupt vessel orbit");

                    return;
                }
            }
            if (driver.reverse)
            {
                driver.referenceBody.position = (!driver.celestialBody ? (Vector3d)driver.driverTransform.position : driver.celestialBody.position) - driver.pos;
            }
            else if (driver.vessel)
            {
                if (VesselPositionSystem.Singleton.VesselHavePositionUpdatesQueued(driver.vessel.id))
                {
                    //DO NOT update the vessel position here. The VesselPositioningSystem takes care of that
                    return;
                }
                else
                {
                    Vector3d vector3d = driver.driverTransform.rotation * driver.vessel.localCoM;
                    driver.vessel.SetPosition((driver.referenceBody.position + driver.pos) - vector3d);
                }
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
