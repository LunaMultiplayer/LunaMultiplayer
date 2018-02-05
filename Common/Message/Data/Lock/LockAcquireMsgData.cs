using Lidgren.Network;
using LunaCommon.Locks;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockAcquireMsgData : LockBaseMsgData
    {
        /// <inheritdoc />
        internal LockAcquireMsgData() { }
        public override LockMessageType LockMessageType => LockMessageType.Acquire;

        public LockDefinition Lock = new LockDefinition();
        public bool Force;

        public override string ClassName { get; } = nameof(LockAcquireMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Force);
            Lock.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Force = lidgrenMsg.ReadBoolean();
            Lock.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + Lock.GetByteCount() + sizeof(bool);
        }
    }
}