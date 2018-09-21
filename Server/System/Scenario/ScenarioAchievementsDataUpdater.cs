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
        /// We received a achievement message so update the scenario file accordingly
        /// </summary>
        public static void WriteAchievementDataToFile(ShareProgressAchievementsMsgData techMsg)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ProgressTracking", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryGetValue("ProgressTracking", out var xmlData)) return;

                    var updatedText = UpdateScenarioWithAchievementData(xmlData, techMsg.Achievements);
                    ScenarioStoreSystem.CurrentScenariosInXmlFormat.TryUpdate("ProgressTracking", updatedText, xmlData);
                }
            });
        }

        /// <summary>
        /// Patches the scenario file with achievement data
        /// </summary>
        private static string UpdateScenarioWithAchievementData(string scenarioData, AchievementInfo[] achievements)
        {
            var document = new XmlDocument();
            document.LoadXml(scenarioData);

            var progressList = document.SelectSingleNode($"/{XmlConverter.StartElement}/{XmlConverter.ParentNode}[@name='Progress']");
            if (progressList != null)
            {
                foreach (var achievement in achievements)
                {
                    var receivedAchievementXmlNode = DeserializeAndImportNode(achievement.Data, achievement.NumBytes, document);
                    if (receivedAchievementXmlNode == null) continue;

                    var existingAchievement = progressList.SelectSingleNode($"{XmlConverter.ParentNode}[@name='{achievement.Id}']");
                    if (existingAchievement != null)
                    {                        
                        //Replace the existing contract values with the received one
                        existingAchievement.InnerXml = receivedAchievementXmlNode.InnerXml;
                    }
                    else
                    {
                        var newAchievement = XmlConverter.CreateXmlNode(achievement.Id, document);
                        newAchievement.InnerXml = receivedAchievementXmlNode.InnerXml;

                        progressList.AppendChild(newAchievement);
                    }

                }
            }

            return document.ToIndentedString();
        }

    }
}
