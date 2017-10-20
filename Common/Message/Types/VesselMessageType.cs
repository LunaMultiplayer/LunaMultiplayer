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
        Update = 7,
        Change = 8,
        Position = 9,
        Flightstate = 10
    }
}