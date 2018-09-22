using LmpCommon.Message.Data.ShareProgress;
using LunaConfigNode.CfgNode;
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

                    var progressNodeHeader = scenario.GetNode("Progress").Value;
                    if (progressNodeHeader != null)
                    {
                        var specificNode = progressNodeHeader.GetNode(achievementMsg.Id);
                        var receivedNode = new ConfigNode(Encoding.UTF8.GetString(achievementMsg.Data, 0, achievementMsg.NumBytes)) { Name = achievementMsg.Id };
                        if (specificNode != null)
                        {
                            progressNodeHeader.ReplaceNode(specificNode.Value, receivedNode);
                        }

                        progressNodeHeader.AddNode(receivedNode);
                    }
                }
            });
        }
    }
}
