using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselsReplyMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselsReplyMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.VesselsReply;
        public KeyValuePair<Guid, byte[]>[] VesselsData { get; set; }
    }
}