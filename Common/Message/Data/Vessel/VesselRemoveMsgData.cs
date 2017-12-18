using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselRemoveMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselRemoveMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Remove;

        public Guid VesselId;

        public override string ClassName { get; } = nameof(VesselRemoveMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            GuidUtil.Serialize(VesselId, lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);
            
            VesselId = GuidUtil.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + GuidUtil.GetByteSize();
        }
    }
}