namespace LunaCommon.Message.Types
{
    public enum GroupMessageType
    {
        Add = 0,
        Remove = 1,
        Invite = 2,
        Accept = 3,
        Kick = 4,
        ListRequest = 5, // list all groups
        ListResponse = 6,
        UpdateRequest = 7, // list information about specific group
        UpdateResponse = 8
    }
}