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
        /// We received a strategy message so update the scenario file accordingly
        /// </summary>
        public static void WriteStrategyDataToFile(StrategyInfo strategy)
        {
            Task.Run(() =>
            {                
                //TODO: Fix this so it uses a replace
                lock (Semaphore.GetOrAdd("StrategySystem", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("StrategySystem", out var scenario)) return;

                    var receivedNode = new ConfigNode(Encoding.UTF8.GetString(strategy.Data, 0, strategy.NumBytes)) { Name = "STRATEGY" };
                    if (receivedNode.IsEmpty()) return;

                    var strategiesNode = scenario.GetNode("STRATEGIES").Value;
                    if (strategiesNode != null)
                    {
                        var strategiesList = strategiesNode.GetNodes("STRATEGY").Select(v => v.Value);
                        var specificstrategyNode = strategiesList.FirstOrDefault(n => n.GetValue("name").Value == strategy.Name);
                        if (specificstrategyNode != null)
                        {
                            strategiesNode.RemoveNode(specificstrategyNode);
                        }

                        strategiesNode.AddNode(receivedNode);
                    }
                }
            });
        }
    }
}
