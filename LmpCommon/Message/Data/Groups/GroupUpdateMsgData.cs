using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Groups
{
    public class GroupUpdateMsgData : GroupBaseMsgData
    {
        /// <inheritdoc />
        internal GroupUpdateMsgData() { }
        public override GroupMessageType GroupMessageType => GroupMessageType.GroupUpdate;

        public Group Group = new Group();

        public override string ClassName { get; } = nameof(GroupUpdateMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            Group.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Group.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + Group.GetByteCount();
        }
    }
}
