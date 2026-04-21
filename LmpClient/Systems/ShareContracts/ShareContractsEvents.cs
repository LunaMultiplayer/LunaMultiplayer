using Contracts;
using Contracts.Templates;
using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Locks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LmpClient.Systems.ShareContracts
{
    public class ShareContractsEvents : SubSystem<ShareContractsSystem>
    {
        /// <summary>
        /// If we get the contract lock then generate contracts
        /// </summary>
        public void LockAcquire(LockDefinition lockDefinition)
        {
            if (lockDefinition.Type == LockType.Contract && lockDefinition.PlayerName == SettingsSystem.CurrentSettings.PlayerName)
            {
                ContractSystem.generateContractIterations = ShareContractsSystem.Singleton.DefaultContractGenerateIterations;
            }
        }

        /// <summary>
        /// Try to get contract lock
        /// </summary>
        public void LockReleased(LockDefinition lockDefinition)
        {
            if (lockDefinition.Type == LockType.Contract)
            {
                System.TryGetContractLock();
            }
        }

        /// <summary>
        /// Try to get contract lock when loading a level
        /// </summary>
        public void LevelLoaded(GameScenes data)
        {
            System.TryGetContractLock();
            // StopIgnoringEvents is deferred to ContractsLoaded() so the guard covers the full
            // ContractSystem.OnLoad() window. onContractsLoaded fires after OnLoad completes,
            // while onLevelWasLoadedGUIReady can fire up to ~100ms before contracts finish loading.
        }

        #region EventHandlers

        public void ContractAccepted(Contract contract)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract accepted: {contract.ContractGuid}");
        }

        public void ContractCancelled(Contract contract)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract cancelled: {contract.ContractGuid}");
        }

        public void ContractCompleted(Contract contract)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract completed: {contract.ContractGuid}");
        }

        public void ContractsListChanged()
        {
            var cs = ContractSystem.Instance;
            if (cs != null)
            {
                var offered  = cs.Contracts.Count(c => c.ContractState == Contract.State.Offered);
                var active   = cs.Contracts.Count(c => c.ContractState == Contract.State.Active);
                var finished = cs.ContractsFinished.Count;
                LunaLog.Log($"[ShareContracts]: Contract list changed — {offered} Offered, {active} Active, {finished} Finished in ContractsFinished.");
            }
            else
            {
                LunaLog.Log("[ShareContracts]: Contract list changed (ContractSystem not available).");
            }
        }

        public void ContractsLoaded()
        {
            LunaLog.Log("Contracts loaded.");
            // Tell the Harmony postfix that onContractsLoaded fired normally so it does not
            // fire it a second time.
            System.ContractsLoadedEventFired = true;

            // Snapshot the server-offered Contract objects NOW, before StopIgnoringEvents() and
            // before any other onContractsLoaded listener (e.g. ContractConfigurator) runs and
            // potentially removes them.  The snapshot is passed to PostLoadContractCheck so it can
            // detect and restore missing contracts in the first frame after load.
            var serverOfferedSnapshot = SnapshotServerOfferedContracts();

            // Safe point to stop ignoring events: ContractSystem.OnLoad() has fully completed.
            // ContractOffered from new contract generation cannot fire until LockAcquire sets
            // generateContractIterations back to the default, which always happens after
            // LevelLoaded/TryGetContractLock. So there is no race between this and LockAcquire.
            System.StopIgnoringEvents();
            LogContractStateBreakdown();
            ReconcileFinishedContracts();
            CreateUnavailableContractStubs();
            LogContractPreLoaderState();
            LogMissionControlTally();
            HighLogic.fetch.StartCoroutine(PostLoadContractCheck(serverOfferedSnapshot));
        }

        /// <summary>
        /// Takes a snapshot of every Contract object whose GUID is in the server's Offered set,
        /// capturing them while they are definitely present (before any post-load listener can
        /// remove them).  The snapshot is used by <see cref="PostLoadContractCheck"/> to detect
        /// and restore contracts silently removed between <c>onContractsLoaded</c> and the first
        /// Unity Update frame.
        /// </summary>
        private List<Contract> SnapshotServerOfferedContracts()
        {
            var cs = ContractSystem.Instance;
            if (cs == null) return new List<Contract>();

            var serverGuids = System.ServerOfferedContractGuids;
            if (serverGuids == null || serverGuids.Count == 0) return new List<Contract>();

            var snapshot = new List<Contract>();
            foreach (var c in cs.Contracts)
            {
                if (c != null && serverGuids.Contains(c.ContractGuid.ToString()))
                    snapshot.Add(c);
            }

            LunaLog.Log($"[ShareContracts]: Snapshotted {snapshot.Count} server-offered contract object(s) for post-load restoration guard.");
            return snapshot;
        }

        /// <summary>
        /// Logs the Offered contract count once per frame for 5 frames after ContractsLoaded(),
        /// then at 1 s and 3 s, to pinpoint exactly when (and therefore which system) removes
        /// contracts that are present at the end of our onContractsLoaded handler.
        ///
        /// At frame +1 it also runs a restoration pass: any contract from
        /// <paramref name="serverOfferedSnapshot"/> that is no longer present in
        /// <c>ContractSystem.Instance.Contracts</c> is re-added with <c>IgnoreEvents</c>
        /// temporarily enabled so the <c>ContractOffered</c> lock-guard cannot withdraw it again.
        /// </summary>
        private System.Collections.IEnumerator PostLoadContractCheck(List<Contract> serverOfferedSnapshot)
        {
            var cs = ContractSystem.Instance;
            if (cs == null) yield break;

            for (int i = 1; i <= 5; i++)
            {
                yield return null;
                var offered = cs.Contracts.Count(c => c.ContractState == Contract.State.Offered);
                LunaLog.Log($"[ShareContracts]: Post-load check frame +{i} — {offered} Offered contracts.");

                if (i == 1 && serverOfferedSnapshot != null && serverOfferedSnapshot.Count > 0)
                {
                    RestoreMissingServerOfferedContracts(cs, serverOfferedSnapshot);
                    // Re-count after restoration so subsequent frames reflect the corrected state.
                    offered = cs.Contracts.Count(c => c.ContractState == Contract.State.Offered);
                    LunaLog.Log($"[ShareContracts]: Post-load check frame +{i} after restoration — {offered} Offered contracts.");
                }

                if (offered == 0) yield break;
            }

            yield return new UnityEngine.WaitForSecondsRealtime(1f);
            {
                var offered = cs.Contracts.Count(c => c.ContractState == Contract.State.Offered);
                LunaLog.Log($"[ShareContracts]: Post-load check +1 s — {offered} Offered contracts.");
            }

            yield return new UnityEngine.WaitForSecondsRealtime(2f);
            {
                var offered = cs.Contracts.Count(c => c.ContractState == Contract.State.Offered);
                LunaLog.Log($"[ShareContracts]: Post-load check +3 s — {offered} Offered contracts.");
            }
        }

        /// <summary>
        /// Restores server-offered contracts that were silently removed from
        /// <c>ContractSystem.Instance.Contracts</c> between <c>onContractsLoaded</c> and the
        /// first Unity Update frame.
        ///
        /// For each snapshotted contract that is no longer in <c>Contracts</c>:
        /// <list type="bullet">
        ///   <item>Searches <c>ContractsFinished</c> first — if found there the contract was
        ///         legitimately completed/failed and no restoration is needed.</item>
        ///   <item>Otherwise re-adds the original Contract object to <c>Contracts</c> with
        ///         <c>IgnoreEvents = true</c> so the <c>ContractOffered</c> lock-guard cannot
        ///         withdraw it immediately.</item>
        /// </list>
        /// </summary>
        private void RestoreMissingServerOfferedContracts(ContractSystem cs, List<Contract> serverOfferedSnapshot)
        {
            // Build a set of GUIDs currently in Contracts (any state) for fast lookup.
            var presentInContracts = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in cs.Contracts)
            {
                if (c != null) presentInContracts.Add(c.ContractGuid.ToString());
            }

            var restored = 0;
            foreach (var contract in serverOfferedSnapshot)
            {
                var guid = contract.ContractGuid.ToString();

                if (presentInContracts.Contains(guid))
                    continue; // still present — nothing to do

                // Check ContractsFinished: if the contract ended up there it completed/failed
                // legitimately (e.g. ExplorationContract auto-completed because the milestone
                // was already achieved in ProgressTracking). Log it and skip.
                var inFinished = false;
                foreach (var fc in cs.ContractsFinished)
                {
                    if (fc != null && fc.ContractGuid.ToString().Equals(guid, StringComparison.OrdinalIgnoreCase))
                    {
                        inFinished = true;
                        LunaLog.Log($"[ShareContracts]: Contract {guid} ({contract.GetType().Name}) is in ContractsFinished " +
                                    $"(state: {fc.ContractState}) — this is correct, not restoring.");
                        break;
                    }
                }

                if (inFinished) continue;

                // The contract was removed from Contracts and is NOT in ContractsFinished —
                // it was silently discarded (e.g. by a slot-limit enforcement mechanism or a
                // WithdrawAndRemoveContract call from a ContractOffered race). Re-add it.
                LunaLog.LogWarning($"[ShareContracts]: Contract {guid} ({contract.GetType().Name}, " +
                                   $"state: {contract.ContractState}) was silently removed from " +
                                   $"ContractSystem.Contracts after load. Restoring.");

                System.StartIgnoringEvents();
                try
                {
                    cs.Contracts.Add(contract);
                    restored++;
                }
                finally
                {
                    System.StopIgnoringEvents();
                }
            }

            if (restored > 0)
            {
                LunaLog.Log($"[ShareContracts]: Restored {restored} silently-removed server Offered contract(s).");
                GameEvents.Contract.onContractsListChanged.Fire();
            }
        }

        /// <summary>
        /// Logs a single summary line showing how many contracts are in each Mission Control tab
        /// (Available = Offered, Active, Archived = ContractsFinished) after the full load and
        /// reconciliation pass has completed, along with how many were dropped due to missing
        /// installed parts or planetary bodies.
        /// </summary>
        private void LogMissionControlTally()
        {
            if (ContractSystem.Instance == null) return;

            var available = 0;
            var active = 0;

            foreach (var c in ContractSystem.Instance.Contracts)
            {
                if (c == null) continue;
                switch (c.ContractState)
                {
                    case Contract.State.Offered: available++; break;
                    case Contract.State.Active:  active++;    break;
                }
            }

            var archived     = ContractSystem.Instance.ContractsFinished.Count;
            var droppedParts = System.LastDroppedMissingPartCount;
            var droppedBodies = System.LastDroppedMissingBodyCount;

            LunaLog.Log($"[LMP]: [ContractSystem] Updated Mission Control, {available} Available, {active} Active, {archived} Archived" +
                        $" ({droppedParts} dropped missing part, {droppedBodies} dropped missing body)");
        }

        /// <summary>
        /// Logs a breakdown of how many contracts are currently in ContractSystem.Instance
        /// split by state, to aid in diagnosing sync issues after load.
        /// </summary>
        private static void LogContractStateBreakdown()
        {
            if (ContractSystem.Instance == null) return;

            var offered = 0;
            var active = 0;
            var finished = 0;
            var other = 0;

            foreach (var c in ContractSystem.Instance.Contracts)
            {
                if (c == null) continue;
                switch (c.ContractState)
                {
                    case Contract.State.Offered:    offered++;  break;
                    case Contract.State.Active:     active++;   break;
                    case Contract.State.Completed:
                    case Contract.State.Failed:
                    case Contract.State.Cancelled:
                    case Contract.State.Withdrawn:
                    case Contract.State.DeadlineExpired:
                        finished++;
                        break;
                    default:
                        other++;
                        break;
                }
            }

            LunaLog.Log($"[ShareContracts]: ContractsLoaded state — " +
                        $"Contracts list: {offered} Offered, {active} Active, {finished} Finished-in-wrong-list, {other} Other | " +
                        $"ContractsFinished list: {ContractSystem.Instance.ContractsFinished.Count}");
        }

        /// <summary>
        /// The server persists Completed/Failed/Cancelled contracts in the CONTRACTS section
        /// until a client update triggers the server-side migrator. KSP's ContractSystem.OnLoad
        /// puts every entry from CONTRACTS into ContractSystem.Contracts regardless of state,
        /// so those finished contracts never reach ContractSystem.ContractsFinished and remain
        /// invisible in the Archive tab.
        ///
        /// This method detects the mismatch and moves every finished contract from Contracts to
        /// ContractsFinished so the Archive tab shows them immediately on connect.
        /// </summary>
        private static void ReconcileFinishedContracts()
        {
            if (ContractSystem.Instance == null) return;

            var toMove = new System.Collections.Generic.List<Contract>();
            foreach (var c in ContractSystem.Instance.Contracts)
            {
                if (c == null || c is LmpUnavailableContract) continue;
                if (c.IsFinished())
                    toMove.Add(c);
            }

            if (toMove.Count == 0) return;

            foreach (var c in toMove)
            {
                ContractSystem.Instance.Contracts.Remove(c);
                ContractSystem.Instance.ContractsFinished.Add(c);
                LunaLog.Log($"[ShareContracts]: Moved finished contract {c.ContractGuid} ({c.GetType().Name}, " +
                            $"state: {c.ContractState}) from Contracts to ContractsFinished.");
            }

            LunaLog.Log($"[ShareContracts]: Reconciled {toMove.Count} finished contract(s) into the Archive list.");
        }

        /// <summary>
        /// After the ContractSystem finishes loading from the server snapshot, compares the set of
        /// contracts that actually loaded against the set that were expected. For each Offered
        /// contract that is absent — dropped by ContractConfigurator due to a missing mod type, or
        /// stripped pre-load due to a missing part — an <see cref="LmpUnavailableContract"/> stub
        /// is added to <see cref="ContractSystem.Instance"/> so the player can see which server
        /// contracts they cannot take on their client.
        /// </summary>
        private void CreateUnavailableContractStubs()
        {
            if (!System.Enabled) return;
            if (ContractSystem.Instance == null) return;

            var pending = System.PendingUnavailableContracts;
            if (pending.Count == 0) return;

            // Map GUID → contract object so we can inspect broken shells, not just presence.
            var loadedContracts = new Dictionary<string, Contract>();
            foreach (var contract in ContractSystem.Instance.Contracts)
            {
                if (contract != null && !(contract is LmpUnavailableContract))
                    loadedContracts[contract.ContractGuid.ToString()] = contract;
            }

            var stubsCreated = 0;
            foreach (var kvp in pending)
            {
                var guid = kvp.Key;
                loadedContracts.TryGetValue(guid, out var loaded);

                bool needsStub;
                if (loaded == null)
                {
                    // Contract was stripped pre-load or completely failed to produce an object.
                    needsStub = true;
                }
                else if (loaded.ParameterCount == 0)
                {
                    // Contract loaded as a parameterless shell — ContractConfigurator could not
                    // find the contract type (missing mod). CC's MeetRequirements() returns false
                    // for these, making them silently invisible. Replace with an informative stub.
                    ContractSystem.Instance.Contracts.Remove(loaded);
                    needsStub = true;
                }
                else
                {
                    needsStub = false;
                }

                if (!needsStub) continue;

                try
                {
                    var stub = BuildUnavailableContractStub(guid, kvp.Value.TypeName, kvp.Value.MissingAsset);
                    if (stub != null)
                    {
                        ContractSystem.Instance.Contracts.Add(stub);
                        stubsCreated++;
                        LunaLog.Log($"[ShareContracts]: Created unavailability stub for {guid} (type: {kvp.Value.TypeName}" +
                                    (kvp.Value.MissingAsset != null ? $", missing part: {kvp.Value.MissingAsset}" : string.Empty) + ").");
                    }
                }
                catch (Exception e)
                {
                    LunaLog.LogError($"[ShareContracts]: Failed to create unavailability stub for {guid}: {e.Message}");
                }
            }

            pending.Clear();

            if (stubsCreated > 0)
            {
                LunaLog.Log($"[ShareContracts]: {stubsCreated} unavailability stub(s) added to the Available contracts list.");
                GameEvents.Contract.onContractsListChanged.Fire();
            }
        }

        private static LmpUnavailableContract BuildUnavailableContractStub(string guid, string typeName, string missingAsset)
        {
            var node = new ConfigNode();
            node.AddValue("guid", guid);
            node.AddValue("prestige", "Trivial");
            node.AddValue("seed", "0");
            node.AddValue("state", "Offered");
            node.AddValue("viewed", "Unseen");
            node.AddValue("deadlineType", "None");
            node.AddValue("expiryType", "None");
            node.AddValue("ignoresWeight", "True");
            node.AddValue("values", "0,0,0,0,0,0,0,0,0,0,0,0");
            node.AddValue(LmpUnavailableContract.OriginalTypeKey, typeName);
            if (missingAsset != null)
                node.AddValue(LmpUnavailableContract.MissingAssetKey, missingAsset);

            return Contract.Load(new LmpUnavailableContract(), node) as LmpUnavailableContract;
        }

        /// <summary>
        /// Reads ContractPreLoader's post-load state for diagnostics.
        /// </summary>
        private static void LogContractPreLoaderState()
        {
            try
            {
                var preLoader = ScenarioRunner.GetLoadedModules()
                    .FirstOrDefault(m => m.GetType().Name == "ContractPreLoader");

                if (preLoader == null)
                {
                    LunaLog.LogWarning("[ContractPreLoader]: ScenarioModule not found in loaded modules — cannot verify injection.");
                    return;
                }

                var node = new ConfigNode();
                preLoader.Save(node);
                LunaLog.Log($"[ContractPreLoader]: Post-load state (type={preLoader.GetType().Name}): {node}");
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[ContractPreLoader]: Error reading post-load state: {e.Message}");
            }
        }

        public void ContractDeclined(Contract contract)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract declined: {contract.ContractGuid}");
        }

        public void ContractFailed(Contract contract)
        {
            if (System.IgnoreEvents) return;

            System.MessageSender.SendContractMessage(contract);
            LunaLog.Log($"Contract failed: {contract.ContractGuid}");
        }

        public void ContractFinished(Contract contract)
        {
            /*
            Doesn't need to be synchronized because there is no ContractFinished state.
            Also the contract will be finished on the contract complete / failed / cancelled / ...
            */
        }

        public void ContractOffered(Contract contract)
        {
            // LmpUnavailableContract stubs are injected by LMP itself — never touch them here.
            if (contract is LmpUnavailableContract) return;

            // Allow contracts being loaded from server data to pass through untouched.
            // IgnoreEvents is set both during ContractUpdate (ShareProgress path) and during
            // ContractSystem.OnLoad() (scenario restore path) via ContractSystem_OnLoad patch.
            if (System.IgnoreEvents) return;

            var guidStr = contract.ContractGuid.ToString();

            // Protect every contract that was part of the server's Offered snapshot.
            // ContractPreLoader (KSPCF) subscribes to onContractsLoaded and re-fires onOffered
            // for contracts already in the system so it can register them in its persistent list.
            // If we intercepted those events we would withdraw valid server contracts regardless
            // of lock status. Contracts not in this set are locally generated and handled normally.
            if (System.ServerOfferedContractGuids.Contains(guidStr))
                return;

            if (!LockSystem.LockQuery.ContractLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName))
            {
                //We don't have the contract lock, so discard any contract KSP generated locally.
                //New generation is already suppressed via generateContractIterations = 0; this
                //is a safety net for any edge case where KSP still fires the event.
                LunaLog.LogWarning($"[ShareContracts]: ContractOffered — withdrawing locally-generated contract " +
                                   $"{guidStr} ({contract.GetType().Name}) — not in server Offered snapshot " +
                                   $"({System.ServerOfferedContractGuids.Count} GUIDs tracked), no contract lock.");
                WithdrawAndRemoveContract(contract);
                return;
            }

            if (contract.GetType().Name == "RecoverAsset")
            {
                //We don't support rescue contracts. See: https://github.com/LunaMultiplayer/LunaMultiplayer/issues/226#issuecomment-431831526
                WithdrawAndRemoveContract(contract);
                return;
            }

            if (contract.GetType().Name == "TourismContract")
            {
                //We don't support tourism contracts.
                WithdrawAndRemoveContract(contract);
                return;
            }

            LunaLog.Log($"Contract offered: {contract.ContractGuid} - {contract.Title}");

            //This should be only called on the client with the contract lock, because it has the generationCount != 0.
            System.MessageSender.SendContractMessage(contract);

            // Push an updated ContractSystem scenario to the server immediately after generation.
            // ContractUpdate messages are fire-and-forget; a player joining before the next
            // 30-second periodic sync would see 0 Offered from the stale server scenario.
            // The call is debounced so the scenario is sent once per generation batch, not once
            // per contract.
            System.ScheduleContractSystemScenarioSend();
        }

        public void ContractParameterChanged(Contract contract, ContractParameter contractParameter)
        {
            //Do not send contract parameter changes as other players might override them
            //See: https://github.com/LunaMultiplayer/LunaMultiplayer/issues/186

            //TODO: Perhaps we can send only when the parameters are complete?
            //if (contractParameter.State == ParameterState.Complete)
            //    System.MessageSender.SendContractMessage(contract);

            LunaLog.Log($"Contract parameter changed on:{contract.ContractGuid}");
        }

        public void ContractRead(Contract contract)
        {
            LunaLog.Log($"Contract read:{contract.ContractGuid}");
        }

        public void ContractSeen(Contract contract)
        {
            LunaLog.Log($"Contract seen:{contract.ContractGuid}");
        }

        #endregion

        /// <summary>
        /// Withdraws a locally-generated contract and removes it from the ContractSystem without
        /// calling Contract.Kill(). Kill() destroys Unity GameObjects that ContractsApp's UIList
        /// may already hold references to, causing a NullReferenceException in UIList.Clear() the
        /// next time the contracts panel is opened. Withdraw() fires onContractsListChanged so the
        /// UI rebuilds cleanly while the entry is still alive, after which the contract is safely
        /// removed from memory.
        /// </summary>
        private static void WithdrawAndRemoveContract(Contract contract)
        {
            contract.Withdraw();
            ContractSystem.Instance.Contracts.Remove(contract);
            contract.Unregister();
        }
    }
}
