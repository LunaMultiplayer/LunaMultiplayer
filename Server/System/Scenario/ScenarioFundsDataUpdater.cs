using System.Globalization;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a funds message so update the scenario file accordingly
        /// </summary>
        public static void WriteFundsDataToFile(double funds)
        {
            _ = Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("Funding", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("Funding", out var scenario)) return;

                    scenario.UpdateValue("funds", funds.ToString(CultureInfo.InvariantCulture));
                }
            });
        }
    }
}
