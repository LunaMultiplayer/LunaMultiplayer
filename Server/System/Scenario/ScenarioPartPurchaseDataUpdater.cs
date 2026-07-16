using LmpCommon.Message.Data.ShareProgress;
using LunaConfigNode.CfgNode;
using Server.Log;
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
            _ = Task.Run(() =>
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
                        {
                            expPartNode?.Value.RemoveValue(experimentalPartMsg.PartName);
                            LunaLog.Debug($"Removing experimental part: {experimentalPartMsg.PartName}");
                        }
                        else
                            specificExpPart.Value = experimentalPartMsg.Count.ToString(CultureInfo.InvariantCulture);
                    }

                    if (expPartNode?.Value.GetAllValues().Count == 0)
                        expPartNode.Value.CreateValue(new CfgNodeValue<string, string>("dummyPart", "0"));
                        // Create a dummy part - LMP treats empty expPartsNode as null, which causes creating a new one
                    ScenarioStoreSystem.BackupScenarios();
                }
            });
        }
    }
}
