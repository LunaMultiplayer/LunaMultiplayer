using LMP.Server.Client;
using LMP.Server.Context;
using LMP.Server.Server;
using LMP.Server.Settings;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Server;

namespace LMP.Server.System
{
    public class HandshakeSystemSender
    {
        public static void SendHandshakeReply(ClientStructure client, HandshakeReply enumResponse, string reason)
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<HandshakeReplyMsgData>();
            msgData.Response = enumResponse;
            msgData.Reason = reason;
            
            if (enumResponse == HandshakeReply.HandshookSuccessfully)
            {
                msgData.ModControlMode = GeneralSettings.SettingsStore.ModControl;
                msgData.ServerStartTime = ServerContext.StartTime;
                if (GeneralSettings.SettingsStore.ModControl != ModControlMode.Disabled)
                {
                    if (!FileHandler.FileExists(ServerContext.ModFilePath))
                        ModFileSystem.GenerateNewModFile();
                    msgData.ModFileData = FileHandler.ReadFile(ServerContext.ModFilePath);
                }
            }

            MessageQueuer.SendToClient<HandshakeSrvMsg>(client, msgData);
        }
    }
}