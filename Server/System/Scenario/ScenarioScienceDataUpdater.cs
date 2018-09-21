using LmpCommon.Xml;
using Server.Utilities;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using LunaConfigNode;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a science message so update the scenario file accordingly
        /// </summary>
        public static void WriteScienceDataToFile(float sciencePoints)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryGetValue("ResearchAndDevelopment", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithScienceData(xmlData, sciencePoints);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryUpdate("ResearchAndDevelopment", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with science data
        /// </summary>
        private static string UpdateScenarioWithScienceData(string scenarioData, float sciencePoints)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var node = document.SelectSingleNode($"/{XmlConverter.StartElement}/{XmlConverter.ValueNode}[@name='sci']");
            if (node != null) node.InnerText = sciencePoints.ToString(CultureInfo.InvariantCulture);

            return document.ToIndentedString();
        }
    }
}
