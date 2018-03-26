using LunaCommon.Xml;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
    }
}
