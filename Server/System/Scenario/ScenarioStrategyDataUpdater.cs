using LmpCommon.Message.Data.ShareProgress;
using Server.Log;
using System;
using System.Linq;
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
            _ = Task.Run(() =>
            {
                try
                {
                    lock (Semaphore.GetOrAdd("StrategySystem", new object()))
                    {
                        if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("StrategySystem", out var scenario)) return;

                        var receivedNode = ParseClientConfigNode(strategy.Data, strategy.NumBytes, "STRATEGY");
                        if (receivedNode.IsEmpty()) return;

                        var strategiesNode = scenario.GetNode("STRATEGIES")?.Value;
                        if (strategiesNode != null)
                        {
                            var strategiesList = strategiesNode.GetNodes("STRATEGY").Select(v => v.Value);
                            var specificstrategyNode = strategiesList.FirstOrDefault(n =>
                            {
                                var name = n.GetValue("name");
                                return name != null && name.Value == strategy.Name;
                            });
                            if (specificstrategyNode != null)
                            {
                                strategiesNode.ReplaceNode(specificstrategyNode, receivedNode);
                            }
                            else
                            {
                                strategiesNode.AddNode(receivedNode);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error updating strategy scenario data: {e}");
                }
            });
        }
    }
}
