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

        TimeSyncing = 6,
        TimeSynced = 7,
        SyncingKerbals = 8,
        KerbalsSynced = 9,
        SyncingSettings = 10,
        SettingsSynced = 11,
        SyncingWarpsubspaces = 12,
        WarpsubspacesSynced = 13,
        SyncingColors = 14,
        ColorsSynced = 15,
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