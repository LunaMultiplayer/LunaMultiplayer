using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Scenario
{
    public class ScenarioBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)ScenarioMessageType;

        public virtual ScenarioMessageType ScenarioMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
