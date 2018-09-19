namespace LmpCommon.Message.Types
{
    public enum CraftMessageType
    {
        FoldersRequest = 0,
        FoldersReply = 1,
        ListRequest = 2,
        ListReply = 3,

        DownloadRequest = 4,
        DeleteRequest = 5,
        CraftData = 6,
        Notification = 7
    }
}
