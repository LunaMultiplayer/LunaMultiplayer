namespace LmpCommon.Message.Types
{
    public enum VesselMessageType
    {
        Proto = 0,
        Remove = 1,
        Position = 2,
        Flightstate = 3,
        Update = 4,
        Resource = 5,
        Sync = 6,
        PartSyncField = 7,
        PartSyncUiField = 8,
        PartSyncCall = 9,
        ActionGroup = 10,
        Fairing = 11,
        Decouple = 12,
        Couple = 13,
        Undock = 14,
    }
}