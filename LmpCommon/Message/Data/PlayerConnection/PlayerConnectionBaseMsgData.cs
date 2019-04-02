using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.PlayerConnection
{
    public abstract class PlayerConnectionBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal PlayerConnectionBaseMsgData() { }
        public override ushort SubType => (ushort)(int)PlayerConnectionMessageType;
        public virtual PlayerConnectionMessageType PlayerConnectionMessageType => throw new NotImplementedException();

        public string PlayerName;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PlayerName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            PlayerName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return PlayerName.GetByteCount();
        }
    }
}
