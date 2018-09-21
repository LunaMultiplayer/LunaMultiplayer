using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Xml;
using Server.Utilities;
using System.Threading.Tasks;
using System.Xml;
using LunaConfigNode;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a science subject message so update the scenario file accordingly
        /// </summary>
        public static void WriteScienceSubjectDataToFile(ScienceSubjectInfo scienceSubject)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryGetValue("ResearchAndDevelopment", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithScienceSubject(xmlData, scienceSubject);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryUpdate("ResearchAndDevelopment", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with science subject data
        /// </summary>
        private static string UpdateScenarioWithScienceSubject(string scenarioData, ScienceSubjectInfo scienceSubject)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var receivedScienceSubjectXmlNode = DeserializeAndImportNode(scienceSubject.Data, scienceSubject.NumBytes, document)?
                .SelectSingleNode($"/{XmlConverter.ParentNode}[@name='Science']");

            if (receivedScienceSubjectXmlNode == null) return document.ToIndentedString();

            var parentNode = document.SelectSingleNode($"/{XmlConverter.StartElement}");
            if (parentNode != null)
            {
                var existingNode = parentNode.SelectSingleNode($"/{XmlConverter.StartElement}/{XmlConverter.ParentNode}[@name='Science']" +
                                                                   $@"/{XmlConverter.ValueNode}[@name='id' and text()=""{scienceSubject.Id}""]" +
                                                                   $"/parent::{XmlConverter.ParentNode}[@name='Science']");

                if (existingNode != null)
                {
                    existingNode.InnerXml = receivedScienceSubjectXmlNode.InnerXml;
                }
                else
                {
                    var importNode = document.ImportNode(receivedScienceSubjectXmlNode, true);
                    parentNode.AppendChild(importNode);
                }
            }

            return document.ToIndentedString();
        }
    }
}
