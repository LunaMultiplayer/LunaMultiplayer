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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            Kerbal.Serialize(lidgrenMsg, dataCompressed);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            Kerbal.Deserialize(lidgrenMsg, dataCompressed);
        }

        public override void Recycle()
        {
            base.Recycle();

            Kerbal.Recycle();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + Kerbal.GetByteCount(dataCompressed);
        }
    }
}