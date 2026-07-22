using LunaConfigNode.CfgNode;
using System.Collections.Generic;
using System.Linq;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// Repairs the in-memory ContractSystem scenario after it has been loaded from disk.
        ///
        /// Older builds of LMP wrote contracts with two structural defects that prevent KSP
        /// from populating its Mission Control "Archived" tab on re-login:
        ///   1. A spurious "CONTRACTS_FINISHED" parent node was created (KSP never produces
        ///      this; it stores both active and finished contracts as siblings under
        ///      "CONTRACTS").
        ///   2. Finished contracts were stored as "CONTRACT" children regardless of state,
        ///      so KSP loaded them as Active and never moved them to the archived list.
        ///
        /// This routine flattens any "CONTRACTS_FINISHED" parent into "CONTRACTS",
        /// renames each child to either "CONTRACT" or "CONTRACT_FINISHED" based on its
        /// serialized state value, and de-duplicates by guid (preferring the finished
        /// entry when both an active and a finished copy exist for the same guid).
        ///
        /// Implementation note: this method rebuilds the CONTRACTS parent's Nodes
        /// collection from scratch (Clear + re-add) instead of using
        /// <c>RemoveNode(ConfigNode)</c>. The latter is broken in LunaConfigNode 1.8.1
        /// (it only updates the lookup dictionaries, not the underlying serialized list),
        /// which would leave duplicate entries on disk that KSP would then load back as
        /// still-active missions in Mission Control.
        ///
        /// It is safe to run on every server start: when the scenario is already in the
        /// correct shape it is a no-op.
        /// </summary>
        public static void MigrateContractsScenario(ConfigNode scenario)
        {
            if (scenario == null) return;

            var contractsParent = scenario.GetNode(ContractsParentNodeName)?.Value;
            if (contractsParent == null)
            {
                scenario.AddNode(new ConfigNode(ContractsParentNodeName, scenario));
                contractsParent = scenario.GetNode(ContractsParentNodeName)?.Value;
                if (contractsParent == null) return;
            }

            var legacyFinishedParent = scenario.GetNode(LegacyFinishedParentNodeName)?.Value;

            var contractCandidates = CollectContractCandidates(contractsParent, legacyFinishedParent);
            var nonContractChildren = CollectNonContractChildren(contractsParent);
            var winners = SelectWinnersByGuid(contractCandidates);

            RebuildContractsParent(contractsParent, nonContractChildren, winners);

            if (legacyFinishedParent != null)
                scenario.RemoveNode(LegacyFinishedParentNodeName);
        }

        private const string LegacyFinishedParentNodeName = "CONTRACTS_FINISHED";

        /// <summary>
        /// Returns every CONTRACT/CONTRACT_FINISHED child found under either the canonical
        /// CONTRACTS parent or the legacy CONTRACTS_FINISHED parent. The returned nodes are
        /// the actual live references; the caller is responsible for renaming them and
        /// re-adding them to the canonical parent.
        /// </summary>
        private static List<ConfigNode> CollectContractCandidates(ConfigNode contractsParent, ConfigNode legacyFinishedParent)
        {
            var candidates = contractsParent.GetAllNodes()
                .Where(IsContractChild)
                .ToList();

            if (legacyFinishedParent != null)
            {
                candidates.AddRange(legacyFinishedParent.GetAllNodes().Where(IsContractChild));
            }

            return candidates;
        }

        /// <summary>
        /// Returns the CONTRACTS parent's children that are NOT contract entries so the
        /// rebuild can preserve them. Today KSP only stores CONTRACT/CONTRACT_FINISHED
        /// here, but preserving unknown children keeps this migration forward-compatible
        /// with future KSP changes or third-party additions.
        /// </summary>
        private static List<ConfigNode> CollectNonContractChildren(ConfigNode contractsParent)
        {
            return contractsParent.GetAllNodes()
                .Where(n => !IsContractChild(n))
                .ToList();
        }

        /// <summary>
        /// De-duplicates by guid, preferring entries whose state indicates a finished
        /// contract (those represent the more advanced lifecycle stage). Each surviving
        /// node has its Name set to the canonical value for its state.
        /// </summary>
        private static List<ConfigNode> SelectWinnersByGuid(List<ConfigNode> candidates)
        {
            var bestByGuid = new Dictionary<string, ConfigNode>();
            var orphans = new List<ConfigNode>();

            foreach (var candidate in candidates)
            {
                var stateValue = candidate.GetValue("state")?.Value;
                candidate.Name = IsFinishedContractState(stateValue) ? FinishedContractNodeName : ActiveContractNodeName;

                var guid = candidate.GetValue("guid")?.Value;
                if (string.IsNullOrEmpty(guid))
                {
                    orphans.Add(candidate);
                    continue;
                }

                if (!bestByGuid.TryGetValue(guid, out var current))
                {
                    bestByGuid[guid] = candidate;
                    continue;
                }

                var candidateIsFinished = candidate.Name == FinishedContractNodeName;
                var currentIsFinished = current.Name == FinishedContractNodeName;
                if (candidateIsFinished && !currentIsFinished)
                    bestByGuid[guid] = candidate;
            }

            var winners = new List<ConfigNode>(bestByGuid.Count + orphans.Count);
            winners.AddRange(bestByGuid.Values);
            winners.AddRange(orphans);
            return winners;
        }

        /// <summary>
        /// Replaces the CONTRACTS parent's children entirely. The clear-then-add pattern
        /// is required because LunaConfigNode 1.8.1's <c>RemoveNode(ConfigNode)</c> does
        /// not remove from the underlying list that drives serialization.
        /// </summary>
        private static void RebuildContractsParent(ConfigNode contractsParent, IEnumerable<ConfigNode> nonContractChildren, IEnumerable<ConfigNode> contractsToKeep)
        {
            contractsParent.Nodes.Clear();

            foreach (var child in nonContractChildren)
                contractsParent.AddNode(child);

            foreach (var contract in contractsToKeep)
                contractsParent.AddNode(contract);
        }

        private static bool IsContractChild(ConfigNode node)
        {
            return node.Name == ActiveContractNodeName || node.Name == FinishedContractNodeName;
        }
    }
}
