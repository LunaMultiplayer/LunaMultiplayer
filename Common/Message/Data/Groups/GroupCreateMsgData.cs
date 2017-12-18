using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupCreateMsgData : GroupBaseMsgData
    {
        /// <inheritdoc />
        internal GroupCreateMsgData() { }
        public override GroupMessageType GroupMessageType => GroupMessageType.CreateGroup;

        public string GroupName;

        public override string ClassName { get; } = nameof(GroupCreateMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(GroupName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            GroupName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + GroupName.GetByteCount();
        }
    }
}
