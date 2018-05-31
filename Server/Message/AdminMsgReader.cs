using System;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Command;
using Server.Context;
using Server.Log;
using Server.Message.Base;
using Server.Server;
using Server.Settings.Structures;

namespace Server.Message
{
    public class AdminMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = (AdminBaseMsgData)message.Data;
            if (!string.IsNullOrEmpty(GeneralSettings.SettingsStore.AdminPassword) && GeneralSettings.SettingsStore.AdminPassword == messageData.AdminPassword)
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<AdminReplyMsgData>();
                switch (messageData.AdminMessageType)
                {
                    case AdminMessageType.Ban:
                        var banMsg = (AdminBanMsgData)message.Data;
                        LunaLog.Debug($"{client.PlayerName}: Requested a ban against {banMsg.PlayerName}. Reason: {banMsg.Reason}");
                        msgData.Response = CommandHandler.Commands["ban"].Func($"{banMsg.PlayerName} {banMsg.Reason}") ? AdminResponse.Ok : AdminResponse.Error;
                        break;
                    case AdminMessageType.Kick:
                        var kickMsg = (AdminKickMsgData)message.Data;
                        LunaLog.Debug($"{client.PlayerName}: Requested a kick against {kickMsg.PlayerName}. Reason: {kickMsg.Reason}");
                        msgData.Response = CommandHandler.Commands["kick"].Func($"{kickMsg.PlayerName} {kickMsg.Reason}") ? AdminResponse.Ok : AdminResponse.Error;
                        break;
                    case AdminMessageType.Dekessler:
                        LunaLog.Debug($"{client.PlayerName}: Requested a dekessler");
                        CommandHandler.Commands["dekessler"].Func(null);
                        msgData.Response = AdminResponse.Ok;
                        break;
                    case AdminMessageType.Nuke:
                        LunaLog.Debug($"{client.PlayerName}: Requested a nuke");
                        CommandHandler.Commands["nukeksc"].Func(null);
                        msgData.Response = AdminResponse.Ok;
                        break;
                    case AdminMessageType.RestartServer:
                        LunaLog.Debug($"{client.PlayerName}: Requested a server restart");
                        CommandHandler.Commands["restartserver"].Func(null);
                        msgData.Response = AdminResponse.Ok;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                MessageQueuer.SendToClient<AdminSrvMsg>(client, msgData);
            }
            else
            {
                LunaLog.Warning($"{client.PlayerName}: Tried to run an admin command with an invalid password");

                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<AdminReplyMsgData>();
                msgData.Response = AdminResponse.InvalidPassword;
                MessageQueuer.SendToClient<AdminSrvMsg>(client, msgData);
            }
        }
    }
}
