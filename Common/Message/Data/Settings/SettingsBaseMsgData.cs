using System;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Settings
{
    public class SettingsBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)SettingsMessageType;

        public virtual SettingsMessageType SettingsMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
