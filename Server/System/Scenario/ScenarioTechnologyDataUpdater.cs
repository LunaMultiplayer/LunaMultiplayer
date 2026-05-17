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
        /// We received a technology message so update the scenario file accordingly
        /// </summary>
        public static void WriteTechnologyDataToFile(ShareProgressTechnologyMsgData techMsg)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                    {
                        if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ResearchAndDevelopment", out var scenario)) return;

                        var receivedNode = ParseClientConfigNode(techMsg.TechNode.Data, techMsg.TechNode.NumBytes, "Tech");
                        if (receivedNode.IsEmpty()) return;

                        var techNodes = scenario.GetNodes("Tech").Select(v => v.Value);
                        var specificTechNode = techNodes.FirstOrDefault(n =>
                        {
                            var id = n.GetValue("id");
                            return id != null && id.Value == techMsg.TechNode.Id;
                        });
                        if (specificTechNode != null) return; //The tech node already exists so quit

                        scenario.AddNode(receivedNode);
                    }
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Error updating technology scenario data: {e}");
                }
            });
        }
    }
}
