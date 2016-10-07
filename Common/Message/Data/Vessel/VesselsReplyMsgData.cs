using System.Collections.Generic;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselsReplyMsgData : VesselBaseMsgData
    {
        public override VesselMessageType VesselMessageType => VesselMessageType.VESSELS_REPLY;
        public KeyValuePair<string, byte[]>[] VesselsData { get; set; }
    }
}