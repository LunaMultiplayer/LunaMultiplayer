using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.PlayerConnection
{
    public class PlayerConnectionBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal PlayerConnectionBaseMsgData() { }
        public override ushort SubType => (ushort)(int)PlayerConnectionMessageType;
        public virtual PlayerConnectionMessageType PlayerConnectionMessageType => throw new NotImplementedException();

        public string PlayerName;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(PlayerName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            PlayerName = lidgrenMsg.ReadString();
        }

        public override void Recycle()
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return PlayerName.GetByteCount();
        }
    }
}
