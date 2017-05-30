using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Chat;
using UnityEngine;

namespace LunaClient.Systems.Chat.Command
{
    public class JoinCommand
    {
        private ChatSystem System => SystemsContainer.Get<ChatSystem>();

        public void JoinChannel(string commandArgs)
        {
            if (commandArgs != "" && commandArgs != "Global" &&
                commandArgs != SettingsSystem.ServerSettings.ConsoleIdentifier &&
                commandArgs != "#Global" && commandArgs != $"#{SettingsSystem.ServerSettings.ConsoleIdentifier}")
            {
                if (commandArgs.StartsWith("#"))
                    commandArgs = commandArgs.Substring(1);
                if (!SystemsContainer.Get<ChatSystem>().JoinedChannels.Contains(commandArgs))
                {
                    LunaLog.Log($"[LMP]: Joining Channel {commandArgs}");
                    SystemsContainer.Get<ChatSystem>().JoinedChannels.Add(commandArgs);
                    SystemsContainer.Get<ChatSystem>().SelectedChannel = commandArgs;
                    SystemsContainer.Get<ChatSystem>().SelectedPmChannel = null;

                    System.MessageSender.SendMessage(new ChatJoinMsgData
                    {
                        Channel = commandArgs,
                        From = SettingsSystem.CurrentSettings.PlayerName
                    });
                }
            }
            else
            {
                ScreenMessages.PostScreenMessage($"Couldn't join '{commandArgs}', Channel Name not valid!");
            }
        }
    }
}