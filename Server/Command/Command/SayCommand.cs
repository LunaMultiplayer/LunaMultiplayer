using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server;
using LunaServer.Command.Command.Base;
using LunaServer.Log;
using LunaServer.Server;
using LunaServer.Settings;

namespace LunaServer.Command.Command
{
    public class SayCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            LunaLog.Normal("Broadcasting " + commandArgs);

            var newMessageData = new ChatChannelMsgData
            {
                SendToAll = true,
                From = GeneralSettings.SettingsStore.ConsoleIdentifier,
                Text = commandArgs
            };

            MessageQueuer.SendToAllClients<ChatSrvMsg>(newMessageData);
        }
    }
}