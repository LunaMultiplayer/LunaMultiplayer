using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Vessel
{
    public class VesselPartSyncCallMsgData : VesselBaseMsgData
    {
        internal VesselPartSyncCallMsgData() { }

        public uint PartFlightId;
        public string ModuleName;
        public string MethodName;

        public override VesselMessageType VesselMessageType => VesselMessageType.PartSyncCall;

        public override string ClassName { get; } = nameof(VesselPartSyncCallMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PartFlightId);
            lidgrenMsg.Write(ModuleName);
            lidgrenMsg.Write(MethodName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PartFlightId = lidgrenMsg.ReadUInt32();
            ModuleName = lidgrenMsg.ReadString();
            MethodName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(uint) + ModuleName.GetByteCount() + MethodName.GetByteCount();
        }
    }
}
