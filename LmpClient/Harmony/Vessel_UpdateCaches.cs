using HarmonyLib;
using LmpClient.Systems.VesselRemoveSys;
using LmpCommon.Enums;
using System;
using System.Collections.Generic;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Safety-net patch for Vessel.UpdateCaches(), which is called by Vessel.FixedUpdate()
    /// every physics tick.  A single broken vessel can produce hundreds of NullReferenceException
    /// [EXC] log entries per second, causing visible lag.
    ///
    /// Prefix   – skips the call entirely for vessels already queued for removal.
    /// Finalizer – catches any NullReferenceException the original method throws, queues the
    ///             vessel for removal, and suppresses the exception so Unity never logs [EXC].
    ///
    /// Using a finalizer instead of trying to pre-validate every field UpdateCaches might
    /// dereference (parts, modules, VesselModules, resources, transforms, …) is intentional:
    /// pre-validation is inherently incomplete and the finalizer catches every case uniformly.
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("UpdateCaches")]
    public class Vessel_UpdateCaches
    {
        private static readonly HashSet<Guid> _killedVessels = new HashSet<Guid>();

        [HarmonyPrefix]
        private static bool PrefixUpdateCaches(Vessel __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            // Already queued for kill — skip the method entirely until the GameObject is destroyed.
            return !_killedVessels.Contains(__instance.id);
        }

        [HarmonyFinalizer]
        private static Exception FinalizerUpdateCaches(Exception __exception, Vessel __instance)
        {
            if (__exception == null) return null;
            if (MainSystem.NetworkState < ClientState.Connected) return __exception;

            if (__exception is NullReferenceException)
            {
                if (_killedVessels.Add(__instance.id))
                {
                    LunaLog.LogError($"[LMP]: Vessel {__instance.id} ({__instance.vesselName}) threw NullReferenceException in Vessel.UpdateCaches — killing to stop [EXC] spam.");
                    VesselRemoveSystem.Singleton.KillVessel(__instance.id, false, "NullReferenceException in Vessel.UpdateCaches");
                }
                return null; // suppress — prevents Unity from logging [EXC]
            }

            return __exception;
        }
    }
}
