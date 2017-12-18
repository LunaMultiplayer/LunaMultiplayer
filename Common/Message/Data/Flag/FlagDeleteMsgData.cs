using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagDeleteMsgData : FlagBaseMsgData
    {
        /// <inheritdoc />
        internal FlagDeleteMsgData() { }
        public override FlagMessageType FlagMessageType => FlagMessageType.FlagDelete;
        
        public string FlagName;

        public override string ClassName { get; } = nameof(FlagDeleteMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(FlagName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            FlagName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + FlagName.GetByteCount();
        }
    }
}