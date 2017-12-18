using Lidgren.Network;
using LunaCommon.Message.Base;
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
            Kerbals = ArrayPool<KerbalInfo>.Claim(KerbalsCount);
            for (var i = 0; i < KerbalsCount; i++)
            {
                if (Kerbals[i] == null)
                    Kerbals[i] = new KerbalInfo();

                Kerbals[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            for (var i = 0; i < KerbalsCount; i++)
            {
                Kerbals[i].Recycle();
            }
            ArrayPool<KerbalInfo>.Release(ref Kerbals);
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