using LMP.Server.Command.Command.Base;
using LMP.Server.Context;
using LMP.Server.Log;
using LMP.Server.Server;
using LMP.Server.Settings;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server;

namespace LMP.Server.Command.Command
{
    public class SayCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            LunaLog.Normal($"Broadcasting {commandArgs}");

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ChatChannelMsgData>();
            msgData.SendToAll = true;
            msgData.From = GeneralSettings.SettingsStore.ConsoleIdentifier;
            msgData.Text = commandArgs;

            MessageQueuer.SendToAllClients<ChatSrvMsg>(msgData);
        }
    }
}