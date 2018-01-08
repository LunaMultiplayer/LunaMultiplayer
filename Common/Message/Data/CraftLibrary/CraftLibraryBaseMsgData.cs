using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public abstract class CraftLibraryBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal CraftLibraryBaseMsgData() { }
        public override ushort SubType => (ushort)(int)CraftMessageType;
        public virtual CraftMessageType CraftMessageType => throw new NotImplementedException();

        public string PlayerName;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(PlayerName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            PlayerName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return PlayerName.GetByteCount();
        }
    }
}