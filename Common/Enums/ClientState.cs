namespace LunaCommon.Enums
{
    public enum ClientState
    {
        DISCONNECTED = 0,
        CONNECTING = 1,
        CONNECTED = 2,
        HANDSHAKING = 3,
        AUTHENTICATED = 4,

        TIME_SYNCING = 5,
        TIME_SYNCED = 6,
        SYNCING_KERBALS = 7,
        KERBALS_SYNCED = 8,
        SYNCING_SETTINGS = 9,
        SETTINGS_SYNCED = 10,
        SYNCING_WARPSUBSPACES = 11,
        WARPSUBSPACES_SYNCED = 12,
        SYNCING_COLORS = 13,
        COLORS_SYNCED = 14,
        SYNCING_PLAYERS = 15,
        PLAYERS_SYNCED = 16,
        SYNCING_SCENARIOS = 17,
        SCNEARIOS_SYNCED = 18,
        SYNCING_CRAFTLIBRARY = 19,
        CRAFTLIBRARY_SYNCED = 20,
        SYNCING_CHAT = 21,
        CHAT_SYNCED = 22,
        SYNCING_LOCKS = 23,
        LOCKS_SYNCED = 24,
        SYNCING_ADMINS = 25,
        ADMINS_SYNCED = 26,
        SYNCING_VESSELS = 27,
        VESSELS_SYNCED = 28,
        TIME_LOCKING = 29,
        TIME_LOCKED = 30,

        STARTING = 31,
        RUNNING = 32
    }
}