using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Facility
{
    public class FacilityBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal FacilityBaseMsgData() { }
        public override ushort SubType => (ushort)(int)FacilityMessageType;
        public virtual FacilityMessageType FacilityMessageType => throw new NotImplementedException();
        public string ObjectId { get; set; }
    }
}