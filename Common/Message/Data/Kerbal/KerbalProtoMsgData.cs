using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalProtoMsgData : KerbalBaseMsgData
    {
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.PROTO;

        public double SendTime { get; set; }
        public string KerbalName { get; set; }
        public byte[] KerbalData { get; set; }
    }
}