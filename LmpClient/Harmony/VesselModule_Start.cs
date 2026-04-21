using HarmonyLib;
using LmpCommon.Enums;
using System;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Safety-net finalizer for VesselModule.Start().
    /// VesselModule.Start() calls the override's OnStart(), and if that throws a
    /// NullReferenceException Unity catches it internally (logging [EXC]) so it never
    /// reaches our code.  Harmony finalizers run after the original method — even when
    /// it threw — so we can intercept the exception here, log a single clear diagnostic,
    /// and suppress it to avoid log spam.
    ///
    /// Known callers this covers:
    ///   CometVessel.OnStart()          — null comet discovery / orbit data on network-loaded vessels
    ///   SuspensionLoadBalancer.OnStart() — null wheel-module reference on network-loaded vessels
    /// </summary>
    [HarmonyPatch(typeof(VesselModule), "Start")]
    public class VesselModule_Start
    {
        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception, VesselModule __instance)
        {
            if (__exception == null) return null;
            if (MainSystem.NetworkState < ClientState.Connected) return __exception;

            if (__exception is NullReferenceException)
            {
                var moduleName  = __instance?.GetType().Name ?? "unknown";
                var gameObjName = __instance?.gameObject?.name ?? "unknown";
                LunaLog.LogWarning($"[LMP]: Suppressed NullReferenceException in {moduleName}.OnStart on '{gameObjName}'. " +
                                   "The vessel was likely loaded with incomplete data from the server.");
                return null;
            }

            return __exception;
        }
    }
}
