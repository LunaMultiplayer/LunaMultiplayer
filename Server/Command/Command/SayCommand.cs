using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server;
using LunaServer.Command.Command.Base;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using LunaServer.Settings;

namespace LunaServer.Command.Command
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