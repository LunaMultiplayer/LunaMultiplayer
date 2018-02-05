using Lidgren.Network;
using LunaCommon.Locks;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockReleaseMsgData : LockBaseMsgData
    {
        /// <inheritdoc />
        internal LockReleaseMsgData() { }
        public override LockMessageType LockMessageType => LockMessageType.Release;

        public LockDefinition Lock = new LockDefinition();
        public bool LockResult;

        public override string ClassName { get; } = nameof(LockReleaseMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(LockResult);
            lidgrenMsg.WritePadBits();

            Lock.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            LockResult = lidgrenMsg.ReadBoolean();
            lidgrenMsg.SkipPadBits();

            Lock.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {            
            //We use sizeof(byte) instead of sizeof(bool) because we use the WritePadBits()
            return base.InternalGetMessageSize() + Lock.GetByteCount() + sizeof(byte);
        }
    }
}