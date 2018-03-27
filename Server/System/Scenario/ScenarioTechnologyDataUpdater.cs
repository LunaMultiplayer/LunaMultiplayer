using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Xml;
using Server.Utilities;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a technology message so update the scenario file accordingly
        /// </summary>
        public static void WriteTechnologyDataToFile(ShareProgressTechnologyMsgData techMsg)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryGetValue("ResearchAndDevelopment", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithTechnologyData(xmlData, techMsg.TechNode);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryUpdate("ResearchAndDevelopment", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with reputation data
        /// </summary>
        private static string UpdateScenarioWithTechnologyData(string scenarioData, TechNodeInfo techNode)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var configNodeData = Encoding.UTF8.GetString(techNode.Data, 0, techNode.NumBytes);

            var newNodeDoc = new XmlDocument();
            newNodeDoc.LoadXml(ConfigNodeXmlParser.ConvertToXml(configNodeData));

            var parentNode = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}");
            if (parentNode != null)
            {
                var newTechXmlNode = newNodeDoc.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='Tech']");
                if (newTechXmlNode != null)
                {
                    var importNode = document.ImportNode(newTechXmlNode, true);
                    parentNode.AppendChild(importNode);
                }
            }

            return document.ToIndentedString();
        }
    }
}
