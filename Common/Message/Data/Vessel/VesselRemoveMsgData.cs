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
        public bool AddToKillList;

        public override string ClassName { get; } = nameof(VesselRemoveMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            base.InternalSerialize(lidgrenMsg, compressData);

            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write(AddToKillList);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);
            
            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            AddToKillList = lidgrenMsg.ReadBoolean();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + GuidUtil.GetByteSize() + sizeof(bool);
        }
    }
}