using Lidgren.Network;
using LunaCommon.Locks;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockListReplyMsgData : LockBaseMsgData
    {
        /// <inheritdoc />
        internal LockListReplyMsgData() { }
        public override LockMessageType LockMessageType => LockMessageType.ListReply;

        public int LocksCount;
        public LockDefinition[] Locks = new LockDefinition[0];

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(LocksCount);
            for (var i = 0; i < LocksCount; i++)
            {
                Locks[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            LocksCount = lidgrenMsg.ReadInt32();
            Locks = ArrayPool<LockDefinition>.Claim(LocksCount);
            for (var i = 0; i < LocksCount; i++)
            {
                if (Locks[i] == null)
                    Locks[i] = new LockDefinition();

                Locks[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            ArrayPool<LockDefinition>.Release(ref Locks);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < LocksCount; i++)
            {
                arraySize += Locks[i].GetByteSize(dataCompressed);
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}