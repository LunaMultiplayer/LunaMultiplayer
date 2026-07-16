using System.Globalization;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a reputation message so update the scenario file accordingly
        /// </summary>
        public static void WriteReputationDataToFile(float reputationPoints)
        {
            _ = Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("Reputation", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("Reputation", out var scenario)) return;

                    scenario.UpdateValue("rep", reputationPoints.ToString(CultureInfo.InvariantCulture));
                }
            });
        }
    }
}
