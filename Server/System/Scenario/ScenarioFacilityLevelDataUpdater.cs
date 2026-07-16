using System.Globalization;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a facility upgrade message so update the scenario file accordingly
        /// </summary>
        public static void WriteFacilityLevelDataToFile(string facilityId, float level)
        {
            _ = Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ScenarioUpgradeableFacilities", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ScenarioUpgradeableFacilities", out var scenario)) return;

                    var facilityNode = scenario.GetNode(facilityId).Value;
                    facilityNode?.UpdateValue("lvl", level.ToString(CultureInfo.InvariantCulture));
                }
            });
        }
    }
}
