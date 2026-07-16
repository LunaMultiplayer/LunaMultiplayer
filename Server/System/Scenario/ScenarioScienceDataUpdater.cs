using System.Globalization;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a science message so update the scenario file accordingly
        /// </summary>
        public static void WriteScienceDataToFile(float sciencePoints)
        {
            _ = Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ResearchAndDevelopment", out var scenario)) return;

                    scenario.UpdateValue("sci", sciencePoints.ToString(CultureInfo.InvariantCulture));
                }
            });
        }
    }
}
