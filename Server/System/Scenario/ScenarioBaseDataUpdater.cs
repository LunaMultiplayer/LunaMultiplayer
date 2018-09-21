using LmpCommon.Xml;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LunaConfigNode;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        #region Semaphore

        /// <summary>
        /// To not overwrite our own data we use a lock
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> Semaphore = new ConcurrentDictionary<string, object>();

        #endregion

        /// <summary>
        /// Raw updates a scenario in the dictionary
        /// </summary>
        public static void RawConfigNodeInsertOrUpdate(string scenarioModule, string scenarioDataInConfigNodeFormat)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd(scenarioModule, new object()))
                {
                    var scenarioAsXml = XmlConverter.ConvertToXml(scenarioDataInConfigNodeFormat);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.AddOrUpdate(scenarioModule, scenarioAsXml, (key, existingVal) => scenarioAsXml);
                }
            });
        }
        
        private static XmlNode DeserializeAndImportNode(byte[] data, int numBytes, XmlDocument docToImportTo)
        {
            var auxDoc = new XmlDocument();
            auxDoc.LoadXml(XmlConverter.ConvertToXml(Encoding.UTF8.GetString(data, 0, numBytes)));
            var newXmlNode = auxDoc.SelectSingleNode($"/{XmlConverter.StartElement}");

            return newXmlNode == null ? null : docToImportTo.ImportNode(newXmlNode, true);
        }
    }
}
