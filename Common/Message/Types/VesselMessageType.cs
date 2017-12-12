namespace LunaCommon.Message.Types
{
    public enum VesselMessageType
    {
        ListRequest = 0,
        ListReply = 1,
        VesselsRequest = 2,
        VesselsReply = 3,
        Proto = 4,
        ProtoReliable = 5,
        Dock = 6,
        Remove = 7,
        Position = 8,
        Flightstate = 9,
        Change = 10
    }
}