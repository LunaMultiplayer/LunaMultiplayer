namespace LunaCommon.Message.Types
{
    public enum VesselMessageType
    {
        ListRequest = 0,
        ListReply = 1,
        VesselsRequest = 2,
        VesselsReply = 3,
        Proto = 4,
        Dock = 5,
        Remove = 6,
        Change = 7,
        Position = 8,
        Flightstate = 9
    }
}