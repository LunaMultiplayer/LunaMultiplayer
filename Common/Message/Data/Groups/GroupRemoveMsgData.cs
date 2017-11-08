using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupRemoveMsgData : GroupBaseMsgData
    {
        /// <inheritdoc />
        internal GroupRemoveMsgData() { }
        public override GroupMessageType GroupMessageType => GroupMessageType.RemoveGroup;
        public string GroupName { get; set; }
    }
}
