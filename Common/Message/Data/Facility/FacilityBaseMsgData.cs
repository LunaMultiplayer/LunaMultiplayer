using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Facility
{
    public abstract class FacilityBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal FacilityBaseMsgData() { }
        public override ushort SubType => (ushort)(int)FacilityMessageType;
        public virtual FacilityMessageType FacilityMessageType => throw new NotImplementedException();

        public string ObjectId;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(ObjectId);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            ObjectId = lidgrenMsg.ReadString();
        }

        public override void Recycle()
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return ObjectId.GetByteCount();
        }
    }
}