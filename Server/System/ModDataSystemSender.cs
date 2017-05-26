using LunaCommon.Message.Data;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Log;
using LunaServer.Server;

namespace LunaServer.System
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
            MessageQueuer.RelayMessage<ModSrvMsg>(excludeClient, new ModMsgData
            {
                Data = messageData,
                ModName = modName
            });
        }

        public static void SendLmpModMessageToClient(ClientStructure client, string modName, byte[] messageData)
        {
            if (modName == null || messageData == null)
            {
                LunaLog.Debug("Attemped to send a null mod message");
                return;
            }

            var newMessageData = new ModMsgData
            {
                Data = messageData,
                ModName = modName
            };
            MessageQueuer.SendToClient<ModSrvMsg>(client, newMessageData);
        }
    }
}