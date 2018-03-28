using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Xml;
using Server.Utilities;
using System.Threading.Tasks;
using System.Xml;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a technology message so update the scenario file accordingly
        /// </summary>
        public static void WriteContractDataToFile(ShareProgressContractsMsgData techMsg)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ContractSystem", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryGetValue("ContractSystem", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithContractData(xmlData, techMsg.Contracts);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryUpdate("ContractSystem", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with reputation data
        /// </summary>
        private static string UpdateScenarioWithContractData(string scenarioData, ContractInfo[] contracts)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var contractsList = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='CONTRACTS']");
            if (contractsList != null)
            {
                foreach (var contract in contracts)
                {
                    var receivedContract = DeserializeAndImportNode(contract.Data, contract.NumBytes, document);
                    if (receivedContract == null) continue;

                    var existingContract = contractsList.SelectSingleNode($"{ConfigNodeXmlParser.ParentNode}[@name='CONTRACT']/" +
                                                                          $@"{ConfigNodeXmlParser.ValueNode}[@name='guid' and text()=""{contract.ContractGuid}""]/" +
                                                                          $"parent::{ConfigNodeXmlParser.ParentNode}[@name='CONTRACT']");
                    if (existingContract != null)
                    {
                        existingContract.InnerXml = receivedContract.InnerXml;  //Replace the existing contract values with the received ones.
                    }
                    else
                    {
                        var newContractNode = ConfigNodeXmlParser.CreateXmlNode("CONTRACT", document);
                        newContractNode.InnerXml = receivedContract.InnerXml;
                        contractsList.AppendChild(newContractNode);
                    }
                }
            }

            return document.ToIndentedString();
        }
    }
}
