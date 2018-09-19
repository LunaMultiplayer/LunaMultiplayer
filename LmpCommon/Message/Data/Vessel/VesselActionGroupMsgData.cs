using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselActionGroupMsgData : VesselBaseMsgData
    {
        internal VesselActionGroupMsgData() { }

        public int ActionGroup;
        public string ActionGroupString;
        public bool Value;

        public override VesselMessageType VesselMessageType => VesselMessageType.ActionGroup;

        public override string ClassName { get; } = nameof(VesselActionGroupMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(ActionGroup);
            lidgrenMsg.Write(ActionGroupString);
            lidgrenMsg.Write(Value);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            ActionGroup = lidgrenMsg.ReadInt32();
            ActionGroupString = lidgrenMsg.ReadString();
            Value = lidgrenMsg.ReadBoolean();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int) + ActionGroupString.GetByteCount() + sizeof(bool);
        }
    }
}
