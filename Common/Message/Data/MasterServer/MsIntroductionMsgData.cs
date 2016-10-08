using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsIntroductionMsgData : MsBaseMsgData
    {
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.INTRODUCTION;

        public long Id { get; set; }
        public string Token { get; set; }
    }
}
