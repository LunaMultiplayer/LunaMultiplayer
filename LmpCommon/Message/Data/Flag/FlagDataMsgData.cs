using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Flag
{
    public class FlagDataMsgData : FlagBaseMsgData
    {
        /// <inheritdoc />
        internal FlagDataMsgData() { }
        public override FlagMessageType FlagMessageType => FlagMessageType.FlagData;

        public FlagInfo Flag = new FlagInfo();

        public override string ClassName { get; } = nameof(FlagDataMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            Flag.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Flag.Deserialize(lidgrenMsg);
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + Flag.GetByteCount();
        }
    }
}