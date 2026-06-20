using LmpCommon.Message.Data.ShareProgress;
using LunaConfigNode.CfgNode;
using Server.Log;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// KSP stores active and finished contracts together under a single CONTRACTS parent.
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
        /// Applies incoming contract updates by state-normalizing each node and upserting by guid.
        /// The CONTRACTS parent is then rebuilt (clear + add) to avoid LunaConfigNode 1.8.1
        /// RemoveNode list-sync issues that can leave stale duplicate entries on disk.
        /// </summary>
        public static void WriteContractDataToFile(ShareProgressContractsMsgData contractsMsg)
        {
            ObserveBackgroundTask(Task.Run(() =>
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
            }));
        }

        private static void ObserveBackgroundTask(Task task)
        {
            if (task == null) return;

            var ignored = task.ContinueWith(
                t => LunaLog.Error($"Background contract update task failed: {t.Exception}"),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
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
