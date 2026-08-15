using HarmonyLib;
using LmpClient.Systems.VesselPositionSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpCommon.Enums;
using System;
using System.Collections.Concurrent;
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
        // Transient NaN orbits are normal during decouple, undock, docking, SOI transitions
        // and the on-rails repack that follows. KSP recovers within a frame or two; if we
        // report-and-kill on the very first NaN we permanently delete a vessel that the
        // game would have fixed on its own. Only act after NaN persists for both a minimum
        // number of consecutive frames and a minimum wall-clock window.
        private const int MinConsecutiveNanFrames = 30;
        private const float MinNanPersistenceSeconds = 3.0f;

        // Self-cleaning dictionary: entries are removed whenever the vessel produces a valid
        // orbit (the common case) or when we actually kill the vessel. Entries can only "leak"
        // if some other LMP system destroys a vessel while it happens to be mid-NaN, which is
        // rare and bounded by total vessels seen in the session (~20 bytes per entry).
        private static readonly ConcurrentDictionary<Guid, NanTracker> NanTrackers = new ConcurrentDictionary<Guid, NanTracker>();

        private sealed class NanTracker
        {
            public int ConsecutiveFrames;
            public float FirstSeenRealtime;
        }

        /// <summary>
        /// We override this method to remove corrupt vessels from the server
        /// </summary>
        [HarmonyPrefix]
        private static bool PrefixUpdateFromParameters(OrbitDriver __instance, ref double ___updateUT)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;
            if (__instance.vessel == null) return true;

            try
            {
                UpdateFromParameters(__instance, ref ___updateUT);
            }
            catch (Exception ex)
            {
                // Never let a failure inside our patch abort the original method's caller
                // (e.g. VesselLoader.LoadVesselIntoGame catches and discards the vessel).
                // Log once per occurrence and fall back to KSP's stock behaviour for this frame.
                Debug.LogError("[LMP - OrbitDriver Patch] Unhandled exception, falling back to stock updateFromParameters: " + ex);
                return true;
            }

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
                if (driver.vessel)
                {
                    if (!ShouldReportPersistentNan(driver.vessel.id))
                    {
                        // Transient NaN: skip the rest of the position update this frame and
                        // let KSP's own SOI/rails handling resolve the orbit on subsequent frames.
                        return;
                    }

                    MonoBehaviour.print(string.Concat(new object[] { "ObT : ", driver.orbit.ObT, "\nM : ", driver.orbit.meanAnomaly, "\nE : ", driver.orbit.eccentricAnomaly, "\nV : ", driver.orbit.trueAnomaly, "\nRadius: ", driver.orbit.radius, "\nvel: ", driver.vel.ToString(), "\nAN: ", driver.orbit.an.ToString(), "\nperiod: ", driver.orbit.period, "\n" }));
                    Debug.LogWarning(string.Concat("[LMP - OrbitDriver Warning!]: ", driver.vessel.vesselName, " had a persistent NaN Orbit and was removed."));
                    NanTrackers.TryRemove(driver.vessel.id, out _);
                    driver.vessel.Unload();

                    // Do NOT send a server remove for a corrupt orbit — the orbit data is a client-side load
                    // issue (bad server ConfigNode) and the server should keep the vessel so it can be
                    // repaired by an admin.  Sending remove here permanently deletes the vessel from the
                    // server and adds it to the in-memory kill-list, making recovery impossible without a
                    // server restart.
                    VesselRemoveSystem.Singleton.KillVessel(driver.vessel.id, false, "Corrupt vessel orbit");

                    return;
                }

                // No vessel attached (e.g. celestial body driver) — preserve original behaviour
                // of logging the diagnostic and bailing out without further position updates.
                MonoBehaviour.print(string.Concat(new object[] { "ObT : ", driver.orbit.ObT, "\nM : ", driver.orbit.meanAnomaly, "\nE : ", driver.orbit.eccentricAnomaly, "\nV : ", driver.orbit.trueAnomaly, "\nRadius: ", driver.orbit.radius, "\nvel: ", driver.vel.ToString(), "\nAN: ", driver.orbit.an.ToString(), "\nperiod: ", driver.orbit.period, "\n" }));
                return;
            }

            if (driver.vessel)
            {
                NanTrackers.TryRemove(driver.vessel.id, out _);
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

        private static bool ShouldReportPersistentNan(Guid vesselId)
        {
            var now = Time.realtimeSinceStartup;
            var tracker = NanTrackers.GetOrAdd(vesselId, _ => new NanTracker
            {
                ConsecutiveFrames = 0,
                FirstSeenRealtime = now
            });

            tracker.ConsecutiveFrames++;
            var elapsed = now - tracker.FirstSeenRealtime;
            return tracker.ConsecutiveFrames >= MinConsecutiveNanFrames && elapsed >= MinNanPersistenceSeconds;
        }
    }
}
