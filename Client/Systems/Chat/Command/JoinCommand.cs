using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Chat;

namespace LunaClient.Systems.Chat.Command
{
    public class JoinCommand
    {
        private ChatSystem System => ChatSystem.Singleton;

        public void JoinChannel(string commandArgs)
        {
            if (commandArgs != "" && commandArgs != "Global" &&
                commandArgs != SettingsSystem.ServerSettings.ConsoleIdentifier &&
                commandArgs != "#Global" && commandArgs != $"#{SettingsSystem.ServerSettings.ConsoleIdentifier}")
            {
                if (commandArgs.StartsWith("#"))
                    commandArgs = commandArgs.Substring(1);
                if (!ChatSystem.Singleton.JoinedChannels.Contains(commandArgs))
                {
                    LunaLog.Log($"[LMP]: Joining Channel {commandArgs}");
                    ChatSystem.Singleton.JoinedChannels.Add(commandArgs);
                    ChatSystem.Singleton.SelectedChannel = commandArgs;
                    ChatSystem.Singleton.SelectedPmChannel = null;

                    var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ChatJoinMsgData>();
                    msgData.From = SettingsSystem.CurrentSettings.PlayerName;
                    msgData.Channel = commandArgs;

                    System.MessageSender.SendMessage(msgData);
                }
            }
            else
            {
                ScreenMessages.PostScreenMessage($"Couldn't join '{commandArgs}', Channel Name not valid!");
            }
        }
    }
}