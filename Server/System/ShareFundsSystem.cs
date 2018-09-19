using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;
using Server.System.Scenario;

namespace Server.System
{
    public static class ShareFundsSystem
    {
        public static void FundsReceived(ClientStructure client, ShareProgressFundsMsgData data)
        {
            LunaLog.Debug($"Funds received: {data.Funds} Reason: {data.Reason}");

            //send the funds update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
            ScenarioDataUpdater.WriteFundsDataToFile(data.Funds);
        }
    }
}
