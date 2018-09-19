using LmpCommon.Xml;
using Server.Utilities;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a funds message so update the scenario file accordingly
        /// </summary>
        public static void WriteFundsDataToFile(double funds)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("Funding", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryGetValue("Funding", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithFundsData(xmlData, funds);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryUpdate("Funding", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with funds data
        /// </summary>
        private static string UpdateScenarioWithFundsData(string scenarioData, double funds)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='funds']");
            if (node != null) node.InnerText = funds.ToString(CultureInfo.InvariantCulture);

            return document.ToIndentedString();
        }
    }
}
