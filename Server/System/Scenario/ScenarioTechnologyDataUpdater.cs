using LmpCommon.Message.Data.ShareProgress;
using LunaConfigNode;
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
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ResearchAndDevelopment", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithTechnologyData(xmlData, techMsg.TechNode);
                    ScenarioStoreSystem.CurrentScenarios.TryUpdate("ResearchAndDevelopment", updatedText, xmlData);
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
            newNodeDoc.LoadXml(XmlConverter.ConvertToXml(configNodeData));

            var parentNode = document.SelectSingleNode($"/{XmlConverter.StartElement}");
            if (parentNode != null)
            {
                var newTechXmlNode = newNodeDoc.SelectSingleNode($"/{XmlConverter.StartElement}/{XmlConverter.ParentNode}[@name='Tech']");
                if (newTechXmlNode != null)
                {
                    var existingNode = parentNode.SelectSingleNode($"/{XmlConverter.StartElement}/{XmlConverter.ParentNode}[@name='Tech']" +
                                                                   $@"/{XmlConverter.ValueNode}[@name='id' and text()=""{techNode.Id}""]" +
                                                                   $"/parent::{XmlConverter.ParentNode}[@name='Tech']");

                    if (existingNode != null)
                    {
                        var parts = newTechXmlNode.SelectNodes($"{XmlConverter.ValueNode}[@name='part']");
                        if (parts != null)
                        {
                            foreach (var part in parts.Cast<XmlNode>())
                            {
                                var existingPart = existingNode.SelectSingleNode($@"{XmlConverter.ValueNode}[@name='part' and text()=""{part.InnerText}""]");
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
