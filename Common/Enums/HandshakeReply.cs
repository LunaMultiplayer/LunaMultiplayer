namespace LunaCommon.Enums
{
    public enum HandshakeReply
    {
        HANDSHOOK_SUCCESSFULLY = 0,
        PROTOCOL_MISMATCH = 1,
        ALREADY_CONNECTED = 2,
        RESERVED_NAME = 3,
        INVALID_KEY = 4,
        PLAYER_BANNED = 5,
        SERVER_FULL = 6,
        NOT_WHITELISTED = 7,
        INVALID_PLAYERNAME = 98,
        MALFORMED_HANDSHAKE = 99
    }
}