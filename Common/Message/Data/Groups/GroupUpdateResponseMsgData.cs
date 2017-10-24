using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupUpdateResponseMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.UpdateResponse;

        public string Owner { get; set; }
        public string[] Members { get; set; }
        public string Name { get; set; }
    }
}
