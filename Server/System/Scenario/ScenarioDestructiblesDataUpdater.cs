using System.Globalization;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a facility destroy/repair message so update the scenario file accordingly
        /// </summary>
        public static void WriteRepairedDestroyedDataToFile(string facilityId, bool intact)
        {
            _ = Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ScenarioDestructibles", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ScenarioDestructibles", out var scenario)) return;

                    var facilityNode = scenario.GetNode(facilityId).Value;
                    facilityNode?.UpdateValue("intact", intact.ToString(CultureInfo.InvariantCulture));
                }
            });
        }
    }
}
