using LunaConfigNode.CfgNode;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        #region Semaphore

        /// <summary>
        /// To not overwrite our own data we use a lock
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> Semaphore = new ConcurrentDictionary<string, object>();

        #endregion

        /// <summary>
        /// Raw updates a scenario in the dictionary
        /// </summary>
        public static void RawConfigNodeInsertOrUpdate(string scenarioModule, string scenarioAsConfigNode)
        {
            Task.Run(() =>
            {
                var scenario = new ConfigNode(scenarioAsConfigNode);
                lock (Semaphore.GetOrAdd(scenarioModule, new object()))
                {
                    ScenarioStoreSystem.CurrentScenarios.AddOrUpdate(scenarioModule, scenario, (key, existingVal) => scenario);
                }
            });
        }
    }
}
