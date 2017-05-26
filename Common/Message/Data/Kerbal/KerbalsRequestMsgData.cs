using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Kerbal
{
    public class KerbalsRequestMsgData : KerbalBaseMsgData
    {
        public override KerbalMessageType KerbalMessageType => KerbalMessageType.Request;
        //Empty message
    }
}