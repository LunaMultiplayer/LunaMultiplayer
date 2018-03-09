using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselEvaMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselEvaMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Eva;
        
        public Guid VesselId;
        public string NewState;
        public string EventToRun;
        
        public override string ClassName { get; } = nameof(VesselEvaMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write(NewState);
            lidgrenMsg.Write(EventToRun);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            NewState = lidgrenMsg.ReadString();
            EventToRun = lidgrenMsg.ReadString();
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + GuidUtil.GetByteSize() + NewState.GetByteCount() + EventToRun.GetByteCount();
        }
    }
}