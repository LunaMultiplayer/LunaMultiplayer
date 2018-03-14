using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselResourceMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselResourceMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Resource;

        public Guid VesselId;

        public int ResourcesCount;
        public VesselResourceInfo[] Resources = new VesselResourceInfo[0];

        public override string ClassName { get; } = nameof(VesselResourceMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            
            GuidUtil.Serialize(VesselId, lidgrenMsg);

            lidgrenMsg.Write(ResourcesCount);
            for (var i = 0; i < ResourcesCount; i++)
            {
                Resources[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            VesselId = GuidUtil.Deserialize(lidgrenMsg);

            ResourcesCount = lidgrenMsg.ReadInt32();
            if (Resources.Length < ResourcesCount)
                Resources = new VesselResourceInfo[ResourcesCount];

            for (var i = 0; i < ResourcesCount; i++)
            {
                if (Resources[i] == null)
                    Resources[i] = new VesselResourceInfo();

                Resources[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < ResourcesCount; i++)
            {
                arraySize += Resources[i]?.GetByteCount() ?? 0;
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}
