using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupUpdateMsgData : GroupBaseMsgData
    {
        /// <inheritdoc />
        internal GroupUpdateMsgData() { }
        public override GroupMessageType GroupMessageType => GroupMessageType.GroupUpdate;

        public Group Group = new Group();

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            Group.Serialize(lidgrenMsg, dataCompressed);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            Group.Deserialize(lidgrenMsg, dataCompressed);
        }

        public override void Recycle()
        {
            base.Recycle();

            Group.Recycle();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + Group.GetByteCount(dataCompressed);
        }
    }
}
