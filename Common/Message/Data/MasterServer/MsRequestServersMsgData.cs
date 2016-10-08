using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsRequestServersMsgData : MsBaseMsgData
    {
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.REQUEST_SERVERS;
    }
}
