using LmpCommon.Message.Data.ShareProgress;
using LunaConfigNode.CfgNode;
using System.Linq;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a part purchase message so update the scenario file accordingly
        /// </summary>
        public static void WritePartPurchaseDataToFile(ShareProgressPartPurchaseMsgData partPurchaseMsg)
        {
            _ = Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ResearchAndDevelopment", out var scenario)) return;

                    var techNodes = scenario.GetNodes("Tech").Select(v => v.Value);
                    var specificTechNode = techNodes.FirstOrDefault(n => n.GetValue("id").Value == partPurchaseMsg.TechId);
                    if (specificTechNode != null)
                    {
                        specificTechNode.CreateValue(new CfgNodeValue<string, string>("part", partPurchaseMsg.PartName));
                    }
                }
            });
        }
    }
}
