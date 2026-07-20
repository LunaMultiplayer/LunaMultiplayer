using LmpCommon.Message.Data.ShareProgress;
using Server.Log;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a science subject message so update the scenario file accordingly
        /// </summary>
        public static void WriteScienceSubjectDataToFile(ScienceSubjectInfo scienceSubject)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                    {
                        if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ResearchAndDevelopment", out var scenario)) return;

                        var receivedNode = ParseClientConfigNode(scienceSubject.Data, scienceSubject.NumBytes, "Science");
                        receivedNode.Parent = scenario;
                        if (receivedNode.IsEmpty()) return;

                        var receivedId = receivedNode.GetValue("id");
                        if (receivedId == null)
                        {
                            LunaLog.Error("Science subject update received with no id — skipping");
                            return;
                        }

                        var scienceNodes = scenario.GetNodes("Science").Select(v => v.Value);
                        var specificNode = scienceNodes.FirstOrDefault(n =>
                        {
                            var id = n.GetValue("id");
                            return id != null && id.Value == receivedId.Value;
                        });
                        if (specificNode != null)
                        {
                            scenario.ReplaceNode(specificNode, receivedNode);
                        }
                        else
                        {
                            scenario.AddNode(receivedNode);
                        }
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error updating science subject scenario data: {e}");
                }
            });
        }
    }
}
