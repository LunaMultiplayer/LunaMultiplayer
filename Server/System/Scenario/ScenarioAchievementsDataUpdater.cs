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

            var progressList = document.SelectSingleNode($"/{ConfigNodeXmlParser.StartElement}/{ConfigNodeXmlParser.ParentNode}[@name='Progress']");
            if (progressList != null)
            {
                foreach (var achievement in achievements)
                {
                    var receivedAchievementXmlNode = DeserializeAndImportNode(achievement.Data, document);
                    if (receivedAchievementXmlNode == null) continue;

                    var existingAchievement = progressList.SelectSingleNode($"{ConfigNodeXmlParser.ParentNode}[@name='{achievement.Id}']");
                    if (existingAchievement != null)
                    {
                        existingAchievement.InnerXml = string.Empty;
                        foreach (var child in receivedAchievementXmlNode.ChildNodes)
                            existingAchievement.AppendChild((XmlNode)child);
                    }
                    else
                    {
                        var newAchievement = ConfigNodeXmlParser.CreateXmlNode(achievement.Id, document);
                        foreach (var child in receivedAchievementXmlNode.ChildNodes)
                            newAchievement.AppendChild((XmlNode)child);

                        progressList.AppendChild(newAchievement);
                    }

                }
            }

            return document.ToIndentedString();
        }

    }
}
