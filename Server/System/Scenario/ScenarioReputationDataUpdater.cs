using LunaConfigNode;
using Server.Utilities;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a reputation message so update the scenario file accordingly
        /// </summary>
        public static void WriteReputationDataToFile(float reputationPoints)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("Reputation", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("Reputation", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithReputationData(xmlData, reputationPoints);
                    ScenarioStoreSystem.CurrentScenarios.TryUpdate("Reputation", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with reputation data
        /// </summary>
        private static string UpdateScenarioWithReputationData(string scenarioData, float reputationPoints)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var node = document.SelectSingleNode($"/{XmlConverter.StartElement}/{XmlConverter.ValueNode}[@name='rep']");
            if (node != null) node.InnerText = reputationPoints.ToString(CultureInfo.InvariantCulture);

            return document.ToIndentedString();
        }
    }
}
