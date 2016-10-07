using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalsRequestMsgData : KerbalBaseMsgData
    {
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.REQUEST;
        //Empty message
    }
}