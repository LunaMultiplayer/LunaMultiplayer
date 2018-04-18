using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Xml;
using Server.Utilities;
using System.Linq;
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
                    var existingNode = parentNode.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='Tech']" +
                                                                   $@"/{ConfigNodeXmlParser.ValueNode}[@name='id' and text()=""{techNode.Id}""]" +
                                                                   $"/parent::{ConfigNodeXmlParser.ParentNode}[@name='Tech']");

                    if (existingNode != null)
                    {
                        var parts = newTechXmlNode.SelectNodes($"{ConfigNodeXmlParser.ValueNode}[@name='part']");
                        if (parts != null)
                        {
                            foreach (var part in parts.Cast<XmlNode>())
                            {
                                var existingPart = existingNode.SelectSingleNode($@"{ConfigNodeXmlParser.ValueNode}[@name='part' and text()=""{part.InnerText}""]");
                                if (existingPart == null)
                                {
                                    var importNode = document.ImportNode(part, true);
                                    existingNode.AppendChild(importNode);
                                }
                            }
                        }
                    }
                    else
                    {
                        var importNode = document.ImportNode(newTechXmlNode, true);
                        parentNode.AppendChild(importNode);
                    }
                }
            }

            return document.ToIndentedString();
        }
    }
}
