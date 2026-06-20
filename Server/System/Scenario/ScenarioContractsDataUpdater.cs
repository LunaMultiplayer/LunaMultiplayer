using LmpCommon.Message.Data.ShareProgress;
using LunaConfigNode.CfgNode;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// Parent node that KSP uses to persist BOTH active and finished contracts.
        /// KSP does not use a separate CONTRACTS_FINISHED parent; instead it stores
        /// finished entries as CONTRACT_FINISHED siblings of CONTRACT entries inside
        /// this single CONTRACTS parent.
        /// </summary>
        private const string ContractsParentNodeName = "CONTRACTS";

        /// <summary>
        /// Child node name KSP expects for active contracts.
        /// </summary>
        private const string ActiveContractNodeName = "CONTRACT";

        /// <summary>
        /// Child node name KSP expects for archived/finished contracts.
        /// Storing finished contracts as CONTRACT here causes them to load as Active
        /// and never appear in the Mission Control "Archived" tab.
        /// </summary>
        private const string FinishedContractNodeName = "CONTRACT_FINISHED";

        /// <summary>
        /// We received a contract message so update the scenario file accordingly.
        ///
        /// Each incoming contract is renamed (CONTRACT vs CONTRACT_FINISHED) based on
        /// its serialized state value, then upserted into a guid-indexed view of the
        /// existing contracts. The CONTRACTS parent is rebuilt from that view via
        /// clear-then-add. This is necessary because LunaConfigNode 1.8.1's
        /// <c>RemoveNode(ConfigNode)</c> overload only updates the lookup dictionaries
        /// and not the underlying serialized list, so any other approach leaves stale
        /// duplicates on disk that KSP loads back as still-active missions.
        /// </summary>
        public static void WriteContractDataToFile(ShareProgressContractsMsgData contractsMsg)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ContractSystem", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ContractSystem", out var scenario)) return;

                    var contractsParent = scenario.GetNode(ContractsParentNodeName)?.Value;
                    if (contractsParent == null)
                    {
                        scenario.AddNode(new ConfigNode(ContractsParentNodeName, scenario));
                        contractsParent = scenario.GetNode(ContractsParentNodeName)?.Value;
                        if (contractsParent == null) return;
                    }

                    var byGuid = IndexExistingContractsByGuid(contractsParent, out var unidentified);
                    var nonContractChildren = CollectNonContractChildren(contractsParent);

                    foreach (var contractInfo in contractsMsg.Contracts)
                    {
                        var incomingNode = new ConfigNode(Encoding.UTF8.GetString(contractInfo.Data, 0, contractInfo.NumBytes));
                        var stateValue = incomingNode.GetValue("state")?.Value;
                        incomingNode.Name = IsFinishedContractState(stateValue) ? FinishedContractNodeName : ActiveContractNodeName;

                        var guid = incomingNode.GetValue("guid")?.Value;
                        if (string.IsNullOrEmpty(guid))
                        {
                            unidentified.Add(incomingNode);
                            continue;
                        }

                        byGuid[guid] = incomingNode;
                    }

                    var survivors = new List<ConfigNode>(byGuid.Count + unidentified.Count);
                    survivors.AddRange(byGuid.Values);
                    survivors.AddRange(unidentified);

                    RebuildContractsParent(contractsParent, nonContractChildren, survivors);
                }
            });
        }

        /// <summary>
        /// Builds a guid-keyed view of the contract children currently under the parent.
        /// Entries without a guid are returned via <paramref name="unidentified"/> so they
        /// can be preserved verbatim during the rebuild.
        /// </summary>
        private static Dictionary<string, ConfigNode> IndexExistingContractsByGuid(ConfigNode contractsParent, out List<ConfigNode> unidentified)
        {
            var index = new Dictionary<string, ConfigNode>();
            unidentified = new List<ConfigNode>();

            foreach (var child in contractsParent.GetAllNodes())
            {
                if (child.Name != ActiveContractNodeName && child.Name != FinishedContractNodeName)
                    continue;

                var guid = child.GetValue("guid")?.Value;
                if (string.IsNullOrEmpty(guid))
                {
                    unidentified.Add(child);
                    continue;
                }

                index[guid] = child;
            }

            return index;
        }

        /// <summary>
        /// Returns true for contract states that KSP persists as CONTRACT_FINISHED.
        /// Mirrors KSP's Contract.State enum members that drive ContractsFinished bucketing.
        /// </summary>
        internal static bool IsFinishedContractState(string state)
        {
            if (string.IsNullOrEmpty(state)) return false;

            switch (state)
            {
                case "Completed":
                case "Cancelled":
                case "DeadlineExpired":
                case "Failed":
                case "Withdrawn":
                    return true;
                default:
                    return false;
            }
        }
    }
}
