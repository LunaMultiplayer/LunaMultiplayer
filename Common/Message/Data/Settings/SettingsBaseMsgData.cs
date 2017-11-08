using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Settings
{
    public class SettingsBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal SettingsBaseMsgData() { }
        public override ushort SubType => (ushort)(int)SettingsMessageType;

        public virtual SettingsMessageType SettingsMessageType => throw new NotImplementedException();
    }
}
