using LmpCommon.Message.Data;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;

namespace Server.System
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