namespace LunaCommon.Message.Types
{
    public enum VesselMessageType
    {
        Proto = 0,
        Dock = 1,
        Remove = 2,
        Position = 3,
        Flightstate = 4,
        Update = 5,
        Resource = 6,
        Sync = 7,
        PartSyncField = 8,
        PartSyncCall = 9,
        ActionGroup = 10,
        Fairing = 11,
        Persistent = 12,
    }
}