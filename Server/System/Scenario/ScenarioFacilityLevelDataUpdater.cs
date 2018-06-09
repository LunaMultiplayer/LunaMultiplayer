using LunaCommon.Xml;
using Server.Utilities;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a facility upgrade message so update the scenario file accordingly
        /// </summary>
        public static void WriteFacilityLevelDataToFile(string facilityId, int level)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ScenarioUpgradeableFacilities", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryGetValue("ScenarioUpgradeableFacilities", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithLevelData(xmlData, facilityId, level);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryUpdate("ScenarioUpgradeableFacilities", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with facility level data
        /// </summary>
        private static string UpdateScenarioWithLevelData(string scenarioData, string facilityId, int level)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/" +
                                                 $"{ConfigNodeXmlParser.ParentNode}[@name='{facilityId}']/" +
                                                 $"{ConfigNodeXmlParser.ValueNode}[@name='lvl']");

            //Valid levels in the scenario file are 0, 0.5 and 1. So for this we divide the arrived level by 2
            if (node != null) node.InnerText = (level/2f).ToString(CultureInfo.InvariantCulture);

            return document.ToIndentedString();
        }
    }
}
