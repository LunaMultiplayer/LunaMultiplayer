namespace LunaCommon.Enums
{
    public enum ClientState
    {
        DisconnectRequested = -1,
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Handshaking = 3,
        Authenticating = 4,
        Authenticated = 5,

        SyncingKerbals = 6,
        KerbalsSynced = 7,
        SyncingSettings = 8,
        SettingsSynced = 9,
        SyncingWarpsubspaces = 10,
        WarpsubspacesSynced = 11,
        SyncingColors = 12,
        ColorsSynced = 13,
        SyncingFlags = 16,
        FlagsSynced = 17,
        SyncingPlayers = 18,
        PlayersSynced = 19,
        SyncingScenarios = 20,
        ScneariosSynced = 21,
        SyncingCraftlibrary = 22,
        CraftlibrarySynced = 23,
        SyncingChat = 24,
        ChatSynced = 25,
        SyncingLocks = 26,
        LocksSynced = 27,
        SyncingAdmins = 28,
        AdminsSynced = 29,
        SyncingGroups = 30,
        GroupsSynced = 31,
        SyncingVessels = 32,
        VesselsSynced = 33,

        Starting = 34,
        Running = 35
    }
}