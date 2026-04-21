using Contracts;
using HarmonyLib;
using LmpCommon.Enums;
using System;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Suppresses exceptions thrown during individual contract loading so that one malformed
    /// contract cannot abort the entire <c>ContractSystem.OnLoadRoutine</c> coroutine.
    ///
    /// Without this, a single contract with an invalid parameter (e.g. a celestial-body index
    /// that is out-of-range on this client) throws an unhandled exception that kills the coroutine
    /// and permanently prevents <c>GameEvents.Contract.onContractsLoaded</c> from firing.
    ///
    /// The pre-load filter in <see cref="LmpClient.Systems.Scenario.ScenarioSystem"/> strips the
    /// most common cases (invalid body index, missing part) before KSP sees the node. This patch
    /// is a second line of defence for edge cases that slip through the filter — for example,
    /// body index values stored in an unexpected format, or parameters from mods that have their
    /// own non-standard serialization quirks.
    ///
    /// Only suppresses when LMP is actively connected to a server. Single-player exceptions
    /// propagate normally to avoid masking unrelated bugs.
    /// </summary>
    [HarmonyPatch(typeof(ContractSystem))]
    [HarmonyPatch("LoadContract")]
    public class ContractSystem_LoadContract
    {
        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (__exception == null) return null;
            if (MainSystem.NetworkState < ClientState.Connected) return __exception;

            LunaLog.LogError($"[ContractSystem_LoadContract]: Suppressed exception during contract loading — " +
                             $"{__exception.GetType().Name}: {__exception.Message}");
            return null;
        }
    }
}
