using LmpCommon.Message.Data.ShareProgress;
using LunaConfigNode.CfgNode;
using System.Globalization;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        /// <summary>
        /// We received a experimental part message so update the scenario file accordingly
        /// </summary>
        public static void WriteExperimentalPartDataToFile(ShareProgressExperimentalPartMsgData experimentalPartMsg)
        {
            Task.Run(() =>
            {
                lock (Semaphore.GetOrAdd("ResearchAndDevelopment", new object()))
                {
                    if (!ScenarioStoreSystem.CurrentScenarios.TryGetValue("ResearchAndDevelopment", out var scenario)) return;

                    var expPartNode = scenario.GetNode("ExpParts");
                    if (expPartNode == null && experimentalPartMsg.Count > 0)
                    {
                        scenario.AddNode(new ConfigNode("ExpParts", scenario));
                        expPartNode = scenario.GetNode("ExpParts");
                    }

                    var specificExpPart = expPartNode?.Value.GetValue(experimentalPartMsg.PartName);
                    if (specificExpPart == null)
                    {
                        var newVal = new CfgNodeValue<string, string>(experimentalPartMsg.PartName,
                            experimentalPartMsg.Count.ToString(CultureInfo.InvariantCulture));

                        expPartNode?.Value.CreateValue(newVal);
                    }
                    else
                    {
                        if (experimentalPartMsg.Count == 0)
                            expPartNode.Value.RemoveValue(specificExpPart.Value);
                        else
                            specificExpPart.Value = experimentalPartMsg.Count.ToString(CultureInfo.InvariantCulture);
                    }

                    if (expPartNode?.Value.GetAllValues().Count == 0)
                        scenario.RemoveNode(expPartNode.Value);
                }
            });
        }
    }
}
