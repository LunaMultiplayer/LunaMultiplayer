namespace LunaCommon.Message.Types
{
    public enum VesselMessageType
    {
        VesselsRequest = 0,
        VesselsReply = 1,
        Proto = 2,
        ProtoReliable = 3,
        Dock = 4,
        Remove = 5,
        Position = 6,
        Flightstate = 7,
        Change = 8
    }
}