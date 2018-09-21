using LmpCommon.Message.Data.ShareProgress;
using LunaConfigNode.CfgNode;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received an achievement message so update the scenario file accordingly
        /// </summary>
        public static void WriteAchievementDataToFile(ShareProgressAchievementsMsgData achievementMsg)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ProgressTracking", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ProgressTracking", out var scenario)) return;

                    var progressNode = scenario.GetNode("Progress").Value;
                    if (progressNode != null)
                    {
                        foreach (var achievement in achievementMsg.Achievements.Select(v=> new ConfigNode(Encoding.UTF8.GetString(v.Data, 0, v.NumBytes))))
                        {
                            achievement.Parent = progressNode;
                            progressNode.CreateOrReplaceNode(progressNode);
                        }
                    }
                }
            });
        }
    }
}
