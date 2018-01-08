using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagDataMsgData : FlagBaseMsgData
    {
        /// <inheritdoc />
        internal FlagDataMsgData() { }
        public override FlagMessageType FlagMessageType => FlagMessageType.FlagData;

        public FlagInfo Flag = new FlagInfo();

        public override string ClassName { get; } = nameof(FlagDataMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            Flag.Serialize(lidgrenMsg, dataCompressed);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            Flag.Deserialize(lidgrenMsg, dataCompressed);
        }
        
        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + Flag.GetByteCount(dataCompressed);
        }
    }
}