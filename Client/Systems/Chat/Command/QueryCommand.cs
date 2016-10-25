using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Systems.Chat.Command
{
    public class QueryCommand
    {
        public void StartQuery(string commandArgs)
        {
            var playerFound = (commandArgs == SettingsSystem.ServerSettings.ConsoleIdentifier) ||
                              StatusSystem.Singleton.PlayerStatusList.ContainsKey(commandArgs);

            if (playerFound && !ChatSystem.Singleton.JoinedPmChannels.Contains(commandArgs))
            {
                Debug.Log("[LMP]: Starting query with " + commandArgs);
                ChatSystem.Singleton.JoinedPmChannels.Add(commandArgs);
                ChatSystem.Singleton.SelectedChannel = null;
                ChatSystem.Singleton.SelectedPmChannel = commandArgs;
            }
            else
            {
                ScreenMessages.PostScreenMessage("Couldn't start query with '" + commandArgs + "', player not found!");
            }
        }
    }
}