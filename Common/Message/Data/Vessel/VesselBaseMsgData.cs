using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public abstract class VesselBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal VesselBaseMsgData() { }
        public override ushort SubType => (ushort)(int)VesselMessageType;
        public virtual VesselMessageType VesselMessageType => throw new NotImplementedException();

        public double GameTime;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(GameTime);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            GameTime = lidgrenMsg.ReadDouble();
        }

        internal override int InternalGetMessageSize()
        {
            return sizeof(double);
        }
    }
}
