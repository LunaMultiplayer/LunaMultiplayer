using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Vessel
{
    public abstract class VesselBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal VesselBaseMsgData() { }
        public override ushort SubType => (ushort)(int)VesselMessageType;
        public virtual VesselMessageType VesselMessageType => throw new NotImplementedException();

        //Avoid using reference types in this message as it can generate allocations and is sent VERY often (specially positions and flight states)
        public Guid VesselId;
        public double GameTime;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            GuidUtil.Serialize(VesselId, lidgrenMsg);
            lidgrenMsg.Write(GameTime);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            VesselId = GuidUtil.Deserialize(lidgrenMsg);
            GameTime = lidgrenMsg.ReadDouble();
        }

        internal override int InternalGetMessageSize()
        {
            return GuidUtil.ByteSize + sizeof(double);
        }
    }
}
