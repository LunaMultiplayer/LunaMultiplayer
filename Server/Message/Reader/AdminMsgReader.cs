using LunaCommon.Enums;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Command;
using Server.Context;
using Server.Log;
using Server.Message.Reader.Base;
using Server.Server;
using Server.Settings;
using System;

namespace Server.Message.Reader
{
    public class AdminMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = (AdminBaseMsgData)message.Data;
            if (!string.IsNullOrEmpty(GeneralSettings.SettingsStore.AdminPassword) && GeneralSettings.SettingsStore.AdminPassword == messageData.AdminPassword)
            {
                switch (messageData.AdminMessageType)
                {
                    case AdminMessageType.Ban:
                        CommandHandler.Commands["ban"].Func(((AdminBanMsgData)message.Data).PlayerName);
                        break;
                    case AdminMessageType.Kick:
                        CommandHandler.Commands["kick"].Func(((AdminKickMsgData)message.Data).PlayerName);
                        break;
                    case AdminMessageType.Dekessler:
                        CommandHandler.Commands["dekessler"].Func(null);
                        break;
                    case AdminMessageType.Nuke:
                        CommandHandler.Commands["nukeksc"].Func(null);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            LunaLog.Warning($"{client.PlayerName}: Tried to run an admin command with an invalid password");

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<AdminReplyMsgData>();
            msgData.Response = AdminResponse.InvalidPassword;
            MessageQueuer.SendToClient<SetingsSrvMsg>(client, msgData);
        }
    }
}
