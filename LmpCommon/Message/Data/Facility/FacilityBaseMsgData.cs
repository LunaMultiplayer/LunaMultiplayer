using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Facility
{
    public abstract class FacilityBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal FacilityBaseMsgData() { }
        public override bool CompressCondition => false;
        public override ushort SubType => (ushort)(int)FacilityMessageType;
        public virtual FacilityMessageType FacilityMessageType => throw new NotImplementedException();

        public string ObjectId;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(ObjectId);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            ObjectId = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return ObjectId.GetByteCount();
        }
    }
}