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
        /// We received a facility destroy/repair message so update the scenario file accordingly
        /// </summary>
        public static void WriteRepairedDestroyedDataToFile(string facilityId, bool intact)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ScenarioDestructibles", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryGetValue("ScenarioDestructibles", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithRepairDestroyedData(xmlData, facilityId, intact);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryUpdate("ScenarioDestructibles", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with facility destroy/repair data
        /// </summary>
        private static string UpdateScenarioWithRepairDestroyedData(string scenarioData, string facilityId, bool intact)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/" +
                                                 $"{ConfigNodeXmlParser.ParentNode}[@name='{facilityId}']/" +
                                                 $"{ConfigNodeXmlParser.ValueNode}[@name='intact']");

            if (node != null) node.InnerText = intact.ToString(CultureInfo.InvariantCulture);

            return document.ToIndentedString();
        }
    }
}
