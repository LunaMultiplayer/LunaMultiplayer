using LmpCommon.Message.Data.ShareProgress;
using Server.Log;
using System;
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
            _ = Task.Run(() =>
            {
                try
                {
                    lock (Semaphore.GetOrAdd("ProgressTracking", new object()))
                    {
                        if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ProgressTracking", out var scenario)) return;

                        var progressNodeHeader = scenario.GetNode("Progress")?.Value;
                        if (progressNodeHeader != null)
                        {
                            var specificNode = progressNodeHeader.GetNode(achievementMsg.Id);
                            var receivedNode = ParseClientConfigNode(achievementMsg.Data, achievementMsg.NumBytes, achievementMsg.Id);
                            if (specificNode != null)
                            {
                                progressNodeHeader.ReplaceNode(specificNode.Value, receivedNode);
                            }
                            else
                            {
                                progressNodeHeader.AddNode(receivedNode);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error updating achievement scenario data: {e}");
                }
            });
        }
    }
}
