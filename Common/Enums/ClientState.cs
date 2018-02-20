namespace LunaCommon.Enums
{
    public enum ClientState
    {
        DisconnectRequested = -1,
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Handshaking = 3,
        HandshakeChallengeReceived = 4,
        Authenticating = 5,
        Authenticated = 6,

        SyncingSettings = 7,
        SettingsSynced = 8,
        SyncingKerbals = 9,
        KerbalsSynced = 10,
        SyncingWarpsubspaces = 11,
        WarpsubspacesSynced = 12,
        SyncingColors = 13,
        ColorsSynced = 14,

        SyncingFlags = 16,
        FlagsSynced = 17,
        SyncingPlayers = 18,
        PlayersSynced = 19,
        SyncingScenarios = 20,
        ScenariosSynced = 21,
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

        Starting = 34,
        Running = 35
    }
}