using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalReplyMsgData : KerbalBaseMsgData
    {
        /// <inheritdoc />
        internal KerbalReplyMsgData() { }
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.Reply;

        public int KerbalsCount;
        public KerbalInfo[] Kerbals = new KerbalInfo[0];

        public override string ClassName { get; } = nameof(KerbalReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(KerbalsCount);
            for (var i = 0; i < KerbalsCount; i++)
            {
                Kerbals[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            KerbalsCount = lidgrenMsg.ReadInt32();
            if (Kerbals.Length < KerbalsCount)
                Kerbals = new KerbalInfo[KerbalsCount];

            for (var i = 0; i < KerbalsCount; i++)
            {
                if (Kerbals[i] == null)
                    Kerbals[i] = new KerbalInfo();

                Kerbals[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }
        
        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < KerbalsCount; i++)
            {
                arraySize += Kerbals[i].GetByteCount(dataCompressed);
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}