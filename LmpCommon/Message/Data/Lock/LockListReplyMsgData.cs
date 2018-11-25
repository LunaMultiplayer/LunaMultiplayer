using Lidgren.Network;
using LmpCommon.Locks;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Lock
{
    public class LockListReplyMsgData : LockBaseMsgData
    {
        /// <inheritdoc />
        internal LockListReplyMsgData() { }
        public override LockMessageType LockMessageType => LockMessageType.ListReply;

        public int LocksCount;
        public LockInfo[] Locks = new LockInfo[0];

        public override string ClassName { get; } = nameof(LockListReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(LocksCount);
            for (var i = 0; i < LocksCount; i++)
            {
                Locks[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            LocksCount = lidgrenMsg.ReadInt32();
            if (Locks.Length < LocksCount)
                Locks = new LockInfo[LocksCount];

            for (var i = 0; i < LocksCount; i++)
            {
                if (Locks[i] == null)
                    Locks[i] = new LockInfo();

                Locks[i].Deserialize(lidgrenMsg);
            }
        }
        
        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < LocksCount; i++)
            {
                arraySize += Locks[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}