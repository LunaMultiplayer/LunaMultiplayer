using LMP.Server.Client;
using LMP.Server.Context;
using LMP.Server.Log;
using LMP.Server.Server;
using LunaCommon.Message.Data;
using LunaCommon.Message.Server;

namespace LMP.Server.System
{
    public class ModDataSystemSender
    {
        public static void SendLmpModMessageToAll(ClientStructure excludeClient, string modName, byte[] messageData)
        {
            if (modName == null || messageData == null)
            {
                LunaLog.Debug("Attemped to send a null mod message");
                return;
            }

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ModMsgData>();
            msgData.Data = messageData;
            msgData.ModName = modName;

            MessageQueuer.RelayMessage<ModSrvMsg>(excludeClient, msgData);
        }

        public static void SendLmpModMessageToClient(ClientStructure client, string modName, byte[] messageData)
        {
            if (modName == null || messageData == null)
            {
                LunaLog.Debug("Attemped to send a null mod message");
                return;
            }

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ModMsgData>();
            msgData.Data = messageData;
            msgData.ModName = modName;
            
            MessageQueuer.SendToClient<ModSrvMsg>(client, msgData);
        }
    }
}