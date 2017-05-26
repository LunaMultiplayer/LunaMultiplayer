namespace LunaCommon.Enums
{
    public enum HandshakeReply
    {
        HandshookSuccessfully = 0,
        ProtocolMismatch = 1,
        AlreadyConnected = 2,
        ReservedName = 3,
        InvalidKey = 4,
        PlayerBanned = 5,
        ServerFull = 6,
        NotWhitelisted = 7,
        InvalidPlayername = 98,
        MalformedHandshake = 99
    }
}