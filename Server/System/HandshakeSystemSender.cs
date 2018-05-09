using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Server;
using Server.Settings.Structures;

namespace Server.System
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
                msgData.ModControl = GeneralSettings.SettingsStore.ModControl;
                msgData.ServerStartTime = ServerContext.StartTime;

                if (GeneralSettings.SettingsStore.ModControl)
                {
                    msgData.ModFileData = FileHandler.ReadFileText(ServerContext.ModFilePath);
                }
            }

            MessageQueuer.SendToClient<HandshakeSrvMsg>(client, msgData);
        }
    }
}
