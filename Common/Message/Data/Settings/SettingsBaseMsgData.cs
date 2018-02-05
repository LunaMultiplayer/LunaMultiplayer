using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Settings
{
    public abstract class SettingsBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal SettingsBaseMsgData() { }
        public override ushort SubType => (ushort)(int)SettingsMessageType;
        public virtual SettingsMessageType SettingsMessageType => throw new NotImplementedException();

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }
        
        internal override int InternalGetMessageSize()
        {
            return 0;
        }
    }
}
