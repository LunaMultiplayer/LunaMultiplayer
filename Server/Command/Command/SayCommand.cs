using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server;
using Server.Command.Command.Base;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings;

namespace Server.Command.Command
{
    public class SayCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            LunaLog.Normal($"Broadcasting {commandArgs}");

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ChatMsgData>();
            msgData.From = GeneralSettings.SettingsStore.ConsoleIdentifier;
            msgData.Text = commandArgs;

            MessageQueuer.SendToAllClients<ChatSrvMsg>(msgData);
        }
    }
}