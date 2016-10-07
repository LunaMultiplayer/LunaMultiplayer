using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Server;
using LunaServer.Settings;

namespace LunaServer.System
{
    public class HandshakeSystemSender
    {
        public static void SendHandshakeReply(ClientStructure client, HandshakeReply enumResponse, string reason)
        {
            var messageData = new HandshakeReplyMsgData
            {
                Response = enumResponse,
                Reason = reason
            };

            if (enumResponse == HandshakeReply.HANDSHOOK_SUCCESSFULLY)
            {
                messageData.ModControlMode = GeneralSettings.SettingsStore.ModControl;
                if (GeneralSettings.SettingsStore.ModControl != ModControlMode.DISABLED)
                {
                    if (!FileHandler.FileExists(ServerContext.ModFilePath))
                        ModFileSystem.GenerateNewModFile();
                    messageData.ModFileData = FileHandler.ReadFile(ServerContext.ModFilePath);
                }
            }

            MessageQueuer.SendToClient<HandshakeSrvMsg>(client, messageData);
        }
    }
}