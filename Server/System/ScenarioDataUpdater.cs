using LunaCommon.Xml;
using Server.Utilities;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

namespace Server.System
{
    public class ScenarioDataUpdater
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
                    var scenarioAsXml = ConfigNodeXmlParser.ConvertToXml(scenarioDataInConfigNodeFormat);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.AddOrUpdate(scenarioModule, scenarioAsXml, (key, existingVal) => scenarioAsXml);
                }
            });
        }

        #region Funds

        /// <summary>
        /// We received a funds message so update the scenario file accordingly
        /// </summary>
        public static void WriteFundsDataToFile(double funds)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("Funding", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryGetValue("Funding", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithFundsData(xmlData, funds);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryUpdate("Funding", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with funds data
        /// </summary>
        /// <returns></returns>
        private static string UpdateScenarioWithFundsData(string scenarioData, double funds)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var node = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ValueNode}[@name='funds']");
            if (node != null) node.InnerText = funds.ToString(CultureInfo.InvariantCulture);

            return document.ToIndentedString();
        }

        #endregion
    }
}
