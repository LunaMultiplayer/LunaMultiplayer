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
        /// We received a technology message so update the scenario file accordingly
        /// </summary>
        public static void WriteTechnologyDataToFile(ShareProgressTechnologyMsgData techMsg)
        {
            Task.Run(() =>
            {                
                //TODO: Fix this so it uses a replace
                lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ResearchAndDevelopment", out var scenario)) return;

                    var receivedNode = new ConfigNode(Encoding.UTF8.GetString(techMsg.TechNode.Data, 0, techMsg.TechNode.NumBytes)) { Name = "Tech" };
                    if (receivedNode.IsEmpty()) return;

                    var techNodes = scenario.GetNodes("Tech").Select(v => v.Value);
                    var specificTechNode = techNodes.FirstOrDefault(n => n.GetValue("id").Value == techMsg.TechNode.Id);
                    if (specificTechNode != null) return; //The science node already exists so quit

                    scenario.AddNode(receivedNode);
                }
            });
        }
    }
}
