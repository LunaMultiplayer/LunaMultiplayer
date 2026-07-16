using LmpCommon.Message.Data.ShareProgress;
using LunaConfigNode.CfgNode;
using System.Collections.Generic;
using System.Text;
using Server.Log;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        // States that mean a contract is done and should live in CONTRACTS_FINISHED, not CONTRACTS.
        // KSP serialises Contract.State enum values by name (e.g. "Completed", not "3").
        private static readonly IReadOnlyCollection<string> FinishedContractStates = new HashSet<string>
        {
            "Completed", "Failed", "Cancelled", "DeadlineExpired", "Withdrawn"
        };

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
        /// Finished contracts are moved from CONTRACTS to CONTRACTS_FINISHED so that
        /// they no longer occupy an offered-contract slot on the server.
        /// </summary>
        public static void WriteContractDataToFile(ShareProgressContractsMsgData contractsMsg)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    lock (Semaphore.GetOrAdd("ContractSystem", new object()))
                    {
                        if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ContractSystem", out var scenario)) return;

                        var contractsNode = scenario.GetNode(ContractsParentNodeName)?.Value;
                        if (contractsNode == null) return;

                        // Get CONTRACTS_FINISHED, creating it if the scenario pre-dates the node.
                        var finishedNodeEntry = scenario.GetNode(FinishedContractNodeName);
                        ConfigNode finishedNode;
                        if (finishedNodeEntry == null)
                        {
                            finishedNode = new ConfigNode("") { Name = FinishedContractNodeName };
                            scenario.AddNode(finishedNode);
                        }
                        else
                        {
                            finishedNode = finishedNodeEntry.Value;
                        }

                        var existingActive   = contractsNode.GetNodes(ActiveContractNodeName).Select(c => c.Value).ToArray();
                        var existingFinished = finishedNode.GetNodes(ActiveContractNodeName).Select(c => c.Value).ToArray();

                        foreach (var contract in contractsMsg.Contracts.Select(v => ParseClientConfigNode(v.Data, v.NumBytes, ActiveContractNodeName)))
                        {
                            var guid  = contract.GetValue("guid")?.Value;
                            var state = contract.GetValue("state")?.Value ?? string.Empty;

                            var inActive   = existingActive.FirstOrDefault(n => n.GetValue("guid")?.Value == guid);
                            var inFinished = existingFinished.FirstOrDefault(n => n.GetValue("guid")?.Value == guid);

                            if (FinishedContractStates.Contains(state))
                            {
                                // Remove from active list so it no longer blocks an offered-contract slot.
                                if (inActive != null)
                                    contractsNode.RemoveNode(inActive);

                                // Upsert into CONTRACTS_FINISHED.
                                if (inFinished != null)
                                    finishedNode.ReplaceNode(inFinished, contract);
                                else
                                    finishedNode.AddNode(contract);
                            }
                            else
                            {
                                // Not finished — update in place within CONTRACTS.
                                if (inActive != null)
                                    contractsNode.ReplaceNode(inActive, contract);
                                else
                                    contractsNode.AddNode(contract);
                            }
                        }
                        var byGuid = IndexExistingContractsByGuid(contractsNode, out var unidentified);
                        var nonContractChildren = CollectNonContractChildren(contractsNode);
                        
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
                        
                        RebuildContractsParent(contractsNode, nonContractChildren, survivors);
                    }

                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error updating contract scenario data: {e}");
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
