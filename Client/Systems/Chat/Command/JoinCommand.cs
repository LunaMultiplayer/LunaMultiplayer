using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Message.Data.Chat;
using UnityEngine;

namespace LunaClient.Systems.Chat.Command
{
    public class JoinCommand
    {
        private ChatSystem System => ChatSystem.Singleton;

        public void JoinChannel(string commandArgs)
        {
            if ((commandArgs != "") && (commandArgs != "Global") &&
                (commandArgs != SettingsSystem.ServerSettings.ConsoleIdentifier) &&
                (commandArgs != "#Global") && (commandArgs != "#" + SettingsSystem.ServerSettings.ConsoleIdentifier))
            {
                if (commandArgs.StartsWith("#"))
                    commandArgs = commandArgs.Substring(1);
                if (!ChatSystem.Singleton.JoinedChannels.Contains(commandArgs))
                {
                    Debug.Log("Joining Channel " + commandArgs);
                    ChatSystem.Singleton.JoinedChannels.Add(commandArgs);
                    ChatSystem.Singleton.SelectedChannel = commandArgs;
                    ChatSystem.Singleton.SelectedPmChannel = null;

                    System.MessageSender.SendMessage(new ChatJoinMsgData
                    {
                        Channel = commandArgs,
                        From = SettingsSystem.CurrentSettings.PlayerName
                    });
                }
            }
            else
            {
                ScreenMessages.PostScreenMessage("Couldn't join '" + commandArgs + "', Channel Name not valid!");
            }
        }
    }
}