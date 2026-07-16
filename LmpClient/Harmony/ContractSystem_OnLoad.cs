using Contracts;
using HarmonyLib;
using LmpClient.Systems.ShareContracts;
using LmpCommon.Enums;
using System.Collections;
using UnityEngine;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Wraps <c>ContractSystem.OnLoad()</c> with <c>IgnoreEvents</c> so that contracts restored
    /// from the server scenario data are not killed by the <c>ContractOffered</c> lock-ownership
    /// check.
    ///
    /// ContractSystem does NOT declare its own OnLoad — the method lives on ScenarioModule.
    /// HarmonyPatch attribute lookup only searches the declared methods of the specified type,
    /// so [HarmonyPatch(typeof(ContractSystem), "OnLoad")] silently finds nothing and skips.
    /// The correct target is ScenarioModule, which is the declaring type. We guard on
    /// __instance type so only the ContractSystem load is affected.
    ///
    /// Additionally, this patch works around a KSP quirk: <c>GameEvents.Contract.onContractsLoaded</c>
    /// only fires during the initial game load from the local save, not when LMP subsequently
    /// re-loads ContractSystem from the server scenario. The postfix starts a monitor coroutine
    /// that waits for the event to fire naturally (inside the async <c>OnLoadRoutine</c>). If it
    /// does not fire within 30 seconds the coroutine fires it manually so ContractPreLoader can
    /// resolve its injected GUIDs and LMP can run post-load reconciliation.
    ///
    /// The prefix intentionally does NOT guard on <see cref="ShareContractsSystem.Enabled"/>. On
    /// the very first LMP scenario reload the system may not yet be enabled when
    /// <c>ScenarioModule.OnLoad</c> is called, but the async <c>OnLoadRoutine</c> coroutine that
    /// does the real work takes several seconds, giving the system time to finish initialising
    /// before the event fires or the coroutine timeout expires.
    /// </summary>
    [HarmonyPatch(typeof(ScenarioModule))]
    [HarmonyPatch("OnLoad")]
    public class ContractSystem_OnLoad
    {
        /// <summary>
        /// Incremented on every ContractSystem OnLoad so that a coroutine from a previous load
        /// cycle knows it has been superseded and exits without firing the event.
        /// </summary>
        private static int _loadGeneration;

        [HarmonyPrefix]
        private static void PrefixOnLoad(ScenarioModule __instance)
        {
            if (!(__instance is ContractSystem)) return;
            if (MainSystem.NetworkState < ClientState.Connected) return;

            var system = ShareContractsSystem.Singleton;
            if (system == null) return;

            LunaLog.Log("[ContractSystem_OnLoad]: Prefix — resetting ContractsLoadedEventFired, starting IgnoreEvents.");
            system.ContractsLoadedEventFired = false;
            system.StartIgnoringEvents();
        }

        [HarmonyPostfix]
        private static void PostfixOnLoad(ScenarioModule __instance)
        {
            if (!(__instance is ContractSystem)) return;
            if (MainSystem.NetworkState < ClientState.Connected) return;

            var system = ShareContractsSystem.Singleton;
            if (system == null) return;

            int generation = ++_loadGeneration;
            LunaLog.Log($"[ContractSystem_OnLoad]: Postfix — OnLoad returned synchronously, starting monitor coroutine (gen {generation}).");
            HighLogic.fetch.StartCoroutine(WaitForContractsLoaded(system, generation));
        }

        /// <summary>
        /// Polls <see cref="ShareContractsSystem.ContractsLoadedEventFired"/> once per frame until
        /// it becomes true (meaning <c>ContractsLoaded()</c> ran via the natural KSP event) or
        /// the timeout expires. On timeout, fires <c>onContractsLoaded</c> manually so that
        /// post-load reconciliation still runs.
        /// </summary>
        private static IEnumerator WaitForContractsLoaded(ShareContractsSystem system, int generation)
        {
            const float TimeoutSeconds = 30f;
            float elapsed = 0f;

            while (!system.ContractsLoadedEventFired && elapsed < TimeoutSeconds)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;

                if (generation != _loadGeneration)
                {
                    LunaLog.Log($"[ContractSystem_OnLoad]: Coroutine gen {generation} superseded by gen {_loadGeneration} — exiting.");
                    yield break;
                }
            }

            if (system.ContractsLoadedEventFired)
            {
                LunaLog.Log($"[ContractSystem_OnLoad]: Coroutine gen {generation} — onContractsLoaded fired naturally after {elapsed:F1}s.");
                yield break;
            }

            // Timeout — the OnLoadRoutine coroutine either crashed or the event was not fired.
            // Fire it manually so ContractPreLoader and LMP reconciliation still run.
            LunaLog.Log($"[ContractSystem_OnLoad]: Coroutine gen {generation} — onContractsLoaded did not fire within {TimeoutSeconds}s, firing manually.");
            GameEvents.Contract.onContractsLoaded.Fire();

            // If ContractsLoaded() ran via the event above it will have set ContractsLoadedEventFired
            // and called StopIgnoringEvents(). If it somehow did not (system not yet enabled /
            // not subscribed), fall back to clearing the IgnoreEvents flag directly so ordinary
            // contract events are not suppressed indefinitely.
            if (!system.ContractsLoadedEventFired)
            {
                LunaLog.LogWarning($"[ContractSystem_OnLoad]: Coroutine gen {generation} — ContractsLoaded() did not run after manual fire " +
                                   $"(system.Enabled={system.Enabled}), stopping IgnoreEvents as fallback.");
                system.StopIgnoringEvents();
            }
        }
    }
}
