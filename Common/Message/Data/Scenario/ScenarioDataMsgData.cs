using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Data.Scenario
{
    public class ScenarioDataMsgData : ScenarioBaseMsgData
    {
        /// <inheritdoc />
        internal ScenarioDataMsgData() { }
        public override ScenarioMessageType ScenarioMessageType => ScenarioMessageType.Data;

        //Array of scenario modules
        //Scenario module1,[Data...]
        //Scenario module2,[Data...]
        //Scenario module3,[Data...]
        public KeyValuePair<string, byte[]>[] ScenarioNameData { get; set; }
    }
}
