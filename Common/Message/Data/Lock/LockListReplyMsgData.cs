using Lidgren.Network;
using LunaCommon.Locks;
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

        public override string ClassName { get; } = nameof(LockListReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            base.InternalSerialize(lidgrenMsg, compressData);

            lidgrenMsg.Write(LocksCount);
            for (var i = 0; i < LocksCount; i++)
            {
                Locks[i].Serialize(lidgrenMsg, compressData);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            LocksCount = lidgrenMsg.ReadInt32();
            if (Locks.Length < LocksCount)
                Locks = new LockDefinition[LocksCount];

            for (var i = 0; i < LocksCount; i++)
            {
                if (Locks[i] == null)
                    Locks[i] = new LockDefinition();

                Locks[i].Deserialize(lidgrenMsg, dataCompressed);
            }
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