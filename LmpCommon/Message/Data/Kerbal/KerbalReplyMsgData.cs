using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Kerbal
{
    public class KerbalReplyMsgData : KerbalBaseMsgData
    {
        /// <inheritdoc />
        internal KerbalReplyMsgData() { }
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.Reply;

        public int KerbalsCount;
        public KerbalInfo[] Kerbals = new KerbalInfo[0];

        public override string ClassName { get; } = nameof(KerbalReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(KerbalsCount);
            for (var i = 0; i < KerbalsCount; i++)
            {
                Kerbals[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            KerbalsCount = lidgrenMsg.ReadInt32();
            if (Kerbals.Length < KerbalsCount)
                Kerbals = new KerbalInfo[KerbalsCount];

            for (var i = 0; i < KerbalsCount; i++)
            {
                if (Kerbals[i] == null)
                    Kerbals[i] = new KerbalInfo();

                Kerbals[i].Deserialize(lidgrenMsg);
            }
        }
        
        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < KerbalsCount; i++)
            {
                arraySize += Kerbals[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}