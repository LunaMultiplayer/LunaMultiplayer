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
        /// We received a science subject message so update the scenario file accordingly
        /// </summary>
        public static void WriteScienceSubjectDataToFile(ScienceSubjectInfo scienceSubject)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ResearchAndDevelopment", out var scenario)) return;

                    var receivedNode = new ConfigNode(Encoding.UTF8.GetString(scienceSubject.Data, 0, scienceSubject.NumBytes)) { Parent = scenario };
                    if (receivedNode.IsEmpty()) return;

                    var techNodes = scenario.GetNodes("Science").Select(v => v.Value);
                    var specificTechNode = techNodes.FirstOrDefault(n => n.GetValue("id").Value == receivedNode.GetValue("id").Value);
                    if (specificTechNode != null)
                    {
                        scenario.ReplaceNode(specificTechNode, receivedNode);
                    }
                    else
                    {
                        scenario.AddNode(receivedNode);
                    }
                }
            });
        }
    }
}
