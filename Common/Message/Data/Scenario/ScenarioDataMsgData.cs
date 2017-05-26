using System.Collections.Generic;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Scenario
{
    public class ScenarioDataMsgData : ScenarioBaseMsgData
    {
        public override ScenarioMessageType ScenarioMessageType => ScenarioMessageType.Data;

        //Array of scenario modules
        //Scenario module1,[Data...]
        //Scenario module2,[Data...]
        //Scenario module3,[Data...]
        public KeyValuePair<string, byte[]>[] ScenarioNameData { get; set; }
    }
}
