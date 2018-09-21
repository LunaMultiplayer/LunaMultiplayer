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
        /// We received a strategy message so update the scenario file accordingly
        /// </summary>
        public static void WriteStrategyDataToFile(StrategyInfo strategy)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("StrategySystem", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryGetValue("StrategySystem", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithStrategyData(xmlData, strategy);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryUpdate("StrategySystem", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with strategy data
        /// </summary>
        private static string UpdateScenarioWithStrategyData(string scenarioData, StrategyInfo strategy)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var strategiesList = document.SelectSingleNode($"/{XmlConverter.StartElement}/{XmlConverter.ParentNode}[@name='STRATEGIES']");
            if (strategiesList != null)
            {
                var receivedStrategy = DeserializeAndImportNode(strategy.Data, strategy.NumBytes, document);
                if (receivedStrategy != null)
                {
                    var existingStrategy = strategiesList.SelectSingleNode($"{XmlConverter.ParentNode}[@name='STRATEGY']/" +
                                                                           $@"{XmlConverter.ValueNode}[@name='name' and text()=""{strategy.Name}""]/" +
                                                                           $"parent::{XmlConverter.ParentNode}[@name='STRATEGY']");
                    if (existingStrategy != null)
                    {
                        //Replace the existing stragegy value with the received one
                        existingStrategy.InnerXml = receivedStrategy.InnerXml;
                    }
                    else
                    {
                        var newStrategyNode = XmlConverter.CreateXmlNode("STRATEGY", document);
                        newStrategyNode.InnerXml = receivedStrategy.InnerXml;
                        strategiesList.AppendChild(newStrategyNode);
                    }
                }
            }

            return document.ToIndentedString();
        }
    }
}
