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

        public override string ClassName { get; } = nameof(GroupUpdateMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            base.InternalSerialize(lidgrenMsg, compressData);

            Group.Serialize(lidgrenMsg, compressData);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            Group.Deserialize(lidgrenMsg, dataCompressed);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + Group.GetByteCount(dataCompressed);
        }
    }
}
