using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalReplyMsgData : KerbalBaseMsgData
    {
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.REPLY;

        public double PlanetTime { get; set; }
        public KeyValuePair<string, byte[]>[] KerbalsData { get; set; }
    }
}