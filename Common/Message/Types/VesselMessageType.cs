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
        Position = 7,
        Flightstate = 8,
        Change = 9
    }
}