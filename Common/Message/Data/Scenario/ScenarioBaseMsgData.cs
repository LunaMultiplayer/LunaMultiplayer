using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Scenario
{
    public class ScenarioBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)ScenarioMessageType;

        public virtual ScenarioMessageType ScenarioMessageType => throw new NotImplementedException();
    }
}
