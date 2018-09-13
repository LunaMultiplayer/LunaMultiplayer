using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
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
