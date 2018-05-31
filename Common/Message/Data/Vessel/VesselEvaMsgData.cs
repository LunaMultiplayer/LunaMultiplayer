using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselEvaMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselEvaMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Eva;
        
        public string NewState;
        public string EventToRun;
        public float LastBoundStep;
        
        public override string ClassName { get; } = nameof(VesselEvaMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(NewState);
            lidgrenMsg.Write(EventToRun);
            lidgrenMsg.Write(LastBoundStep);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            NewState = lidgrenMsg.ReadString();
            EventToRun = lidgrenMsg.ReadString();
            LastBoundStep = lidgrenMsg.ReadFloat();
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + NewState.GetByteCount() + EventToRun.GetByteCount() + sizeof(float);
        }
    }
}
