using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Kerbal
{
    public class KerbalRemoveMsgData : KerbalBaseMsgData
    {
        /// <inheritdoc />
        internal KerbalRemoveMsgData() { }
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.Remove;

        public string KerbalName;

        public override string ClassName { get; } = nameof(KerbalRemoveMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(KerbalName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            KerbalName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + KerbalName.GetByteCount();
        }
    }
}