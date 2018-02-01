using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalProtoMsgData : KerbalBaseMsgData
    {
        /// <inheritdoc />
        internal KerbalProtoMsgData() { }
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.Proto;
        
        public KerbalInfo Kerbal = new KerbalInfo();

        public override string ClassName { get; } = nameof(KerbalProtoMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            Kerbal.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Kerbal.Deserialize(lidgrenMsg);
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + Kerbal.GetByteCount();
        }
    }
}