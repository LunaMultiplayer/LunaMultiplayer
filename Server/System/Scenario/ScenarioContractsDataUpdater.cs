using LmpCommon.Message.Data.ShareProgress;
using LunaConfigNode.CfgNode;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a contract message so update the scenario file accordingly
        /// </summary>
        public static void WriteContractDataToFile(ShareProgressContractsMsgData contractsMsg)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ContractSystem", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ContractSystem", out var scenario)) return;

                    var scenariosParentNode = scenario.GetNode("CONTRACTS")?.Value;
                    if (scenariosParentNode == null) return;

                    var existingContracts = scenariosParentNode.GetNodes("CONTRACT").Select(c=> c.Value).ToArray();
                    if (existingContracts.Any())
                    {
                        foreach (var contract in contractsMsg.Contracts.Select(v => new ConfigNode(Encoding.UTF8.GetString(v.Data, 0, v.NumBytes)) { Name = "CONTRACT" }))
                        {
                            var specificContractNode = existingContracts.FirstOrDefault(n => n.GetValue("guid").Value == contract.GetValue("guid").Value);
                            if (specificContractNode != null)
                            {
                                scenariosParentNode.ReplaceNode(specificContractNode, contract);
                            }
                            else
                            {
                                scenariosParentNode.AddNode(contract);
                            }
                        }
                    }
                }
            });
        }
    }
}
