using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsIntroductionMsgData : MsBaseMsgData
    {
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.Introduction;

        public long Id { get; set; }
        public string InternalEndpoint { get; set; }
        public string Token { get; set; }
    }
}
