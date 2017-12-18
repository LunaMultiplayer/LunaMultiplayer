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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(LockResult);
            Lock.Serialize(lidgrenMsg, dataCompressed);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            LockResult = lidgrenMsg.ReadBoolean();
            Lock.Deserialize(lidgrenMsg, dataCompressed);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + Lock.GetByteSize(dataCompressed) + sizeof(bool);
        }
    }
}