using Contracts;
using LmpClient.Events;
using LmpClient.Systems.Lock;
using LmpClient.Systems.Scenario;
using LmpClient.Systems.ShareProgress;
using LmpCommon.Enums;
using System;
using System.Collections;
using System.Collections.Generic;

namespace LmpClient.Systems.ShareContracts
{
    public class ShareContractsSystem : ShareProgressBaseSystem<ShareContractsSystem, ShareContractsMessageSender, ShareContractsMessageHandler>
    {
        public override string SystemName { get; } = nameof(ShareContractsSystem);

        private ShareContractsEvents ShareContractsEvents { get; } = new ShareContractsEvents();

        public int DefaultContractGenerateIterations;

        /// <summary>
        /// Populated by <see cref="ScenarioSystem"/> just before the ContractSystem scenario is
        /// loaded. Keys are GUID strings of Offered contracts in the server's snapshot; values are
        /// the original contract type name and, for part-validation failures, the missing part name.
        ///
        /// <see cref="ShareContractsEvents.ContractsLoaded"/> compares this set against what
        /// actually loaded into <see cref="ContractSystem.Instance"/> and creates
        /// <see cref="LmpUnavailableContract"/> stubs for any GUIDs that are absent.
        /// </summary>
        internal Dictionary<string, (string TypeName, string MissingAsset)> PendingUnavailableContracts { get; }
            = new Dictionary<string, (string, string)>();

        /// <summary>
        /// All Offered contract GUIDs from the most recently received ContractSystem scenario
        /// snapshot. Populated in <see cref="ScenarioMessageHandler"/> when the ContractSystem
        /// data arrives (before any scenario loading), so that
        /// <see cref="ScenarioSystem.LoadScenarioDataIntoGame"/> can inject every offered GUID
        /// into the ContractPreLoader scenario node and make all server contracts visible in
        /// Mission Control — not just the small saved subset.
        ///
        /// Written from the network thread; read from the Unity main thread during loading.
        /// The write always finishes hundreds of milliseconds before the read begins (scenarios
        /// are queued in a single batch before loading starts), so no lock is required.
        /// </summary>
        internal HashSet<string> ServerOfferedContractGuids { get; private set; }
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal void SetServerOfferedContractGuids(List<string> guids)
        {
            ServerOfferedContractGuids = guids != null
                ? new HashSet<string>(guids, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Full ConfigNodes for every Offered contract in the most recently received
        /// ContractSystem snapshot. Saved in <see cref="ScenarioMessageHandler"/> alongside
        /// <see cref="ServerOfferedContractGuids"/> so that
        /// <see cref="ScenarioSystem.LoadScenarioDataIntoGame"/> can inject complete contract
        /// data into the ContractPreLoader scenario node before KSPCF's ContractPreLoader.OnLoad
        /// processes it.
        ///
        /// KSPCF's patched <c>GenerateContracts</c> uses ContractPreLoader's stored list as a
        /// whitelist: contracts in the list are restored (or kept if already present); contracts
        /// not in the list are cleared.  With an empty list every server contract is cleared.
        /// Injecting the full nodes (type, guid, state, all parameters) allows KSPCF to
        /// successfully restore all 43 server contracts even when
        /// <c>generateContractIterations == 0</c>.
        ///
        /// Same write-before-read threading guarantee as <see cref="ServerOfferedContractGuids"/>.
        /// </summary>
        internal List<ConfigNode> ServerOfferedContractNodes { get; private set; }
            = new List<ConfigNode>();

        internal void SetServerOfferedContractNodes(List<ConfigNode> nodes)
        {
            ServerOfferedContractNodes = nodes ?? new List<ConfigNode>();
        }

        /// <summary>
        /// Set to <c>true</c> by <see cref="ShareContractsEvents.ContractsLoaded"/> the moment it
        /// runs, so that <see cref="ContractSystem_OnLoad"/> can detect whether KSP fired
        /// <c>GameEvents.Contract.onContractsLoaded</c> during the server-scenario re-load.
        ///
        /// Reset to <c>false</c> by the <see cref="ContractSystem_OnLoad"/> Harmony prefix
        /// immediately before each ContractSystem.OnLoad call so the check is always fresh.
        /// </summary>
        internal bool ContractsLoadedEventFired { get; set; }

        /// <summary>
        /// Number of contracts dropped from the last received ContractSystem snapshot because
        /// they referenced a part not installed on this client.
        /// </summary>
        internal int LastDroppedMissingPartCount { get; private set; }

        /// <summary>
        /// Number of contracts dropped from the last received ContractSystem snapshot because
        /// they referenced a planetary body index that does not exist on this client.
        /// </summary>
        internal int LastDroppedMissingBodyCount { get; private set; }

        /// <summary>
        /// Records the Offered contracts from the ContractSystem scenario node so that stubs can
        /// be created for any that fail to load. Called from <see cref="ScenarioSystem"/> before
        /// the node is handed to KSP.
        /// </summary>
        /// <param name="scenarioNode">The ContractSystem ConfigNode received from the server.</param>
        /// <param name="strippedWithMissingPart">
        /// Map of GUID → missing asset description for contracts already stripped by the
        /// pre-filter. Values whose <c>MissingAsset</c> starts with <c>"part '"</c> were dropped
        /// for a missing part; all others were dropped for an invalid planetary body index.
        /// May be null if pre-filtering did not run.
        /// </param>
        public void PrepareUnavailableContractStubs(ConfigNode scenarioNode,
            IReadOnlyDictionary<string, (string TypeName, string MissingAsset)> strippedWithMissingPart)
        {
            PendingUnavailableContracts.Clear();
            LastDroppedMissingPartCount = 0;
            LastDroppedMissingBodyCount = 0;

            var contractsNode = scenarioNode.GetNode("CONTRACTS");
            if (contractsNode != null)
            {
                foreach (var contractNode in contractsNode.GetNodes("CONTRACT"))
                {
                    var guid = contractNode.GetValue("guid");
                    var typeName = contractNode.GetValue("type") ?? "Unknown";
                    var state = contractNode.GetValue("state");

                    if (string.IsNullOrEmpty(guid) || state != "Offered") continue;

                    PendingUnavailableContracts[guid] = (typeName, null);
                }
            }

            // Stripped contracts were removed from the node before KSP sees them, so they never
            // appear in the iteration above. Track them separately so stubs are created for them.
            if (strippedWithMissingPart != null)
            {
                foreach (var kvp in strippedWithMissingPart)
                {
                    PendingUnavailableContracts[kvp.Key] = kvp.Value;

                    if (kvp.Value.MissingAsset != null && kvp.Value.MissingAsset.StartsWith("part '"))
                        LastDroppedMissingPartCount++;
                    else
                        LastDroppedMissingBodyCount++;
                }
            }

            LunaLog.Log($"[ShareContracts]: Tracking {PendingUnavailableContracts.Count} Offered contracts from server snapshot for unavailability detection.");
        }

        //This queue system is not used because we use one big queue in ShareCareerSystem for this system.
        protected override bool ShareSystemReady => true;

        protected override GameMode RelevantGameModes => GameMode.Career;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            if (!CurrentGameModeIsRelevant) return;

            ContractSystem.generateContractIterations = 0;

            // Protect the startup window: any ContractOffered events that fire between
            // system enable and the scene being GUI-ready (when TryGetContractLock runs)
            // must not kill server contracts. This matters on servers with no active lock
            // holder, where contracts can only arrive via ContractSystem.OnLoad() and any
            // post-load re-offer events (e.g. from ContractPreLoader or mod initialisation).
            // Cleared in LevelLoaded() after lock status is determined.
            IgnoreEvents = true;

            LockEvent.onLockAcquire.Add(ShareContractsEvents.LockAcquire);
            LockEvent.onLockRelease.Add(ShareContractsEvents.LockReleased);
            GameEvents.onLevelWasLoadedGUIReady.Add(ShareContractsEvents.LevelLoaded);

            GameEvents.Contract.onAccepted.Add(ShareContractsEvents.ContractAccepted);
            GameEvents.Contract.onCancelled.Add(ShareContractsEvents.ContractCancelled);
            GameEvents.Contract.onCompleted.Add(ShareContractsEvents.ContractCompleted);
            GameEvents.Contract.onContractsListChanged.Add(ShareContractsEvents.ContractsListChanged);
            GameEvents.Contract.onContractsLoaded.Add(ShareContractsEvents.ContractsLoaded);
            GameEvents.Contract.onDeclined.Add(ShareContractsEvents.ContractDeclined);
            GameEvents.Contract.onFailed.Add(ShareContractsEvents.ContractFailed);
            GameEvents.Contract.onFinished.Add(ShareContractsEvents.ContractFinished);
            GameEvents.Contract.onOffered.Add(ShareContractsEvents.ContractOffered);
            GameEvents.Contract.onParameterChange.Add(ShareContractsEvents.ContractParameterChanged);
            GameEvents.Contract.onRead.Add(ShareContractsEvents.ContractRead);
            GameEvents.Contract.onSeen.Add(ShareContractsEvents.ContractSeen);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            PendingUnavailableContracts.Clear();
            ServerOfferedContractNodes.Clear();
            _contractSystemScenariaSendPending = false;
            ContractSystem.generateContractIterations = DefaultContractGenerateIterations;

            LockEvent.onLockAcquire.Remove(ShareContractsEvents.LockAcquire);
            LockEvent.onLockRelease.Remove(ShareContractsEvents.LockReleased);
            GameEvents.onLevelWasLoadedGUIReady.Remove(ShareContractsEvents.LevelLoaded);

            //Always try to remove the event, as when we disconnect from a server the server settings will get the default values
            GameEvents.Contract.onAccepted.Remove(ShareContractsEvents.ContractAccepted);
            GameEvents.Contract.onCancelled.Remove(ShareContractsEvents.ContractCancelled);
            GameEvents.Contract.onCompleted.Remove(ShareContractsEvents.ContractCompleted);
            GameEvents.Contract.onContractsListChanged.Remove(ShareContractsEvents.ContractsListChanged);
            GameEvents.Contract.onContractsLoaded.Remove(ShareContractsEvents.ContractsLoaded);
            GameEvents.Contract.onDeclined.Remove(ShareContractsEvents.ContractDeclined);
            GameEvents.Contract.onFailed.Remove(ShareContractsEvents.ContractFailed);
            GameEvents.Contract.onFinished.Remove(ShareContractsEvents.ContractFinished);
            GameEvents.Contract.onOffered.Remove(ShareContractsEvents.ContractOffered);
            GameEvents.Contract.onParameterChange.Remove(ShareContractsEvents.ContractParameterChanged);
            GameEvents.Contract.onRead.Remove(ShareContractsEvents.ContractRead);
            GameEvents.Contract.onSeen.Remove(ShareContractsEvents.ContractSeen);
        }

        private bool _contractSystemScenariaSendPending;

        /// <summary>
        /// Schedules a one-frame-deferred send of the ContractSystem scenario to the server.
        /// Safe to call many times within the same frame — only one send is dispatched per batch.
        /// Ensures joining players see the lock holder's current Offered contracts even before
        /// the next 30-second periodic scenario sync runs.
        /// </summary>
        internal void ScheduleContractSystemScenarioSend()
        {
            if (_contractSystemScenariaSendPending) return;
            _contractSystemScenariaSendPending = true;
            HighLogic.fetch.StartCoroutine(SendContractSystemScenarioDeferred());
        }

        private IEnumerator SendContractSystemScenarioDeferred()
        {
            yield return null;
            _contractSystemScenariaSendPending = false;
            ScenarioSystem.Singleton?.SendScenarioModuleImmediate("ContractSystem");
        }

        /// <summary>
        /// Try to acquire the contract lock
        /// </summary>
        public void TryGetContractLock()
        {
            if (!LockSystem.LockQuery.ContractLockExists())
            {
                LockSystem.Singleton.AcquireContractLock();
            }
        }
    }
}
