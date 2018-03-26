using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;

namespace Server.System
{
    public static class ShareContractsSystem
    {
        public static void ContractsReceived(ClientStructure client, ShareProgressContractsMsgData data)
        {
            LunaLog.Debug("Contract data received:");

            foreach (var item in data.Contracts)
            {
                LunaLog.Debug(item.ContractGuid.ToString());
            }

            //send the contract update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
        }
    }
}
