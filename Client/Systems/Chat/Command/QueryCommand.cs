using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using UnityEngine;

namespace LunaClient.Systems.Chat.Command
{
    public class QueryCommand
    {
        public void StartQuery(string commandArgs)
        {
            var playerFound = commandArgs == SettingsSystem.ServerSettings.ConsoleIdentifier ||
                              SystemsContainer.Get<StatusSystem>().PlayerStatusList.ContainsKey(commandArgs);

            if (playerFound && !SystemsContainer.Get<ChatSystem>().JoinedPmChannels.Contains(commandArgs))
            {
                Debug.Log($"[LMP]: Starting query with {commandArgs}");
                SystemsContainer.Get<ChatSystem>().JoinedPmChannels.Add(commandArgs);
                SystemsContainer.Get<ChatSystem>().SelectedChannel = null;
                SystemsContainer.Get<ChatSystem>().SelectedPmChannel = commandArgs;
            }
            else
            {
                ScreenMessages.PostScreenMessage($"Couldn't start query with '{commandArgs}', player not found!");
            }
        }
    }
}