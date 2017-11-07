namespace LunaCommon.Enums
{
    public enum ClientState
    {
        DisconnectRequested = -1,
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Handshaking = 3,
        Authenticated = 4,

        TimeSyncing = 5,
        TimeSynced = 6,
        SyncingKerbals = 7,
        KerbalsSynced = 8,
        SyncingSettings = 9,
        SettingsSynced = 10,
        SyncingWarpsubspaces = 11,
        WarpsubspacesSynced = 12,
        SyncingColors = 13,
        ColorsSynced = 14,
        SyncingFlags = 15,
        FlagsSynced = 16,
        SyncingPlayers = 17,
        PlayersSynced = 18,
        SyncingScenarios = 19,
        ScneariosSynced = 20,
        SyncingCraftlibrary = 21,
        CraftlibrarySynced = 22,
        SyncingChat = 23,
        ChatSynced = 24,
        SyncingLocks = 25,
        LocksSynced = 26,
        SyncingAdmins = 27,
        AdminsSynced = 28,
        SyncingGroups = 29,
        GroupsSynced = 30,
        SyncingVessels = 31,
        VesselsSynced = 32,

        Starting = 33,
        Running = 34
    }
}