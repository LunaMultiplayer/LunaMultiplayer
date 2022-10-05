namespace LmpCommon.Message.Types
{
    public enum MasterServerMessageSubType
    {
        RegisterServer = 0,
        RequestServers = 1,
        ReplyServers = 2,
        Introduction = 3,
        // We impelement a STUN-inspired public address + port and NAT type discovery system
        STUNBindingRequest = 4,
        STUNSuccessResponse = 5
    }
}
