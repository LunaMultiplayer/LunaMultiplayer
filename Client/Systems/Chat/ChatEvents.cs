using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Windows.Chat;
using LunaCommon.Message.Data.Chat;
using System.Collections.Generic;

namespace LunaClient.Systems.Chat
{
    public class ChatEvents : SubSystem<ChatSystem>
    {
        private static ChatWindow Screen => ChatWindow.Singleton;

        public void HandleChatEvents()
        {
            //Handle leave event
            if (!System.LeaveEventHandled)
                HandleLeaveEvent();
            //Handle send event
            if (!System.SendEventHandled)
            {
                if (System.SendText != "")
                    System.InputHandler.HandleInput(System.SendText);
                System.SendText = "";
                System.SendEventHandled = true;
            }
            //Handle join messages
            while (System.Queuer.NewJoinMessages.Count > 0)
                HandlePlayerJoinEvent();
            //Handle leave messages
            while (System.Queuer.NewLeaveMessages.Count > 0)
                HandlePlayerLeaveEvent();
            //Handle Channel messages
            while (System.Queuer.NewChannelMessages.Count > 0)
                HandleChannelMessageEvent();
            //Handle private messages
            while (System.Queuer.NewPrivateMessages.Count > 0)
                HandlePrivateMessageEvent();
            //Handle console messages
            while (System.Queuer.NewConsoleMessages.Count > 0)
                HandleConsoleMessageEvent();
            while (System.Queuer.DisconnectingPlayers.Count > 0)
                HandlePlayerDisconnectEvent();
        }

        #region Handlers

        private void HandlePlayerDisconnectEvent()
        {
            if (System.Queuer.DisconnectingPlayers.TryDequeue(out var disconnectingPlayer))
            {
                if (System.PlayerChannels.ContainsKey(disconnectingPlayer))
                    System.PlayerChannels.Remove(disconnectingPlayer);
                if (System.JoinedPmChannels.Contains(disconnectingPlayer))
                    System.JoinedPmChannels.Remove(disconnectingPlayer);
                if (System.HighlightPm.Contains(disconnectingPlayer))
                    System.HighlightPm.Remove(disconnectingPlayer);
                if (System.PrivateMessages.ContainsKey(disconnectingPlayer))
                    System.PrivateMessages.Remove(disconnectingPlayer);
            }
        }

        private void HandleConsoleMessageEvent()
        {
            if (System.Queuer.NewConsoleMessages.TryDequeue(out var ce))
            {
                //Highlight if the Channel isn't Selected.
                if (System.SelectedChannel != SettingsSystem.ServerSettings.ConsoleIdentifier)
                    if (!System.HighlightChannel.Contains(SettingsSystem.ServerSettings.ConsoleIdentifier))
                        System.HighlightChannel.Add(SettingsSystem.ServerSettings.ConsoleIdentifier);
                //Move the bar to the bottom on a new Message
                if (System.SelectedChannel != null && System.SelectedPmChannel == null &&
                    SettingsSystem.ServerSettings.ConsoleIdentifier == System.SelectedChannel)
                {
                    Screen.ChatScrollPos.y = float.PositiveInfinity;
                    if (System.ChatLocked)
                        System.SelectTextBox = true;
                }
                System.ConsoleMessages.Add(ce.Message);
            }
        }

        private void HandlePrivateMessageEvent()
        {
            if (System.Queuer.NewPrivateMessages.TryDequeue(out var pe))
            {
                if (pe.FromPlayer != SettingsSystem.CurrentSettings.PlayerName)
                {
                    if (!System.PrivateMessages.ContainsKey(pe.FromPlayer))
                        System.PrivateMessages.Add(pe.FromPlayer, new List<string>());
                    //Highlight if the player isn't Selected
                    if (!System.JoinedPmChannels.Contains(pe.FromPlayer))
                        System.JoinedPmChannels.Add(pe.FromPlayer);
                    if (System.SelectedPmChannel != pe.FromPlayer)
                        if (!System.HighlightPm.Contains(pe.FromPlayer))
                            System.HighlightPm.Add(pe.FromPlayer);
                }
                if (!System.PrivateMessages.ContainsKey(pe.ToPlayer))
                    System.PrivateMessages.Add(pe.ToPlayer, new List<string>());
                //Move the bar to the bottom on a new Message
                if (System.SelectedPmChannel != null && System.SelectedChannel == null &&
                    (pe.FromPlayer == System.SelectedPmChannel ||
                     pe.FromPlayer == SettingsSystem.CurrentSettings.PlayerName))
                {
                    Screen.ChatScrollPos.y = float.PositiveInfinity;
                    if (System.ChatLocked)
                        System.SelectTextBox = true;
                }
                if (pe.FromPlayer != SettingsSystem.CurrentSettings.PlayerName)
                    System.PrivateMessages[pe.FromPlayer].Add($"{pe.FromPlayer}: {pe.Message}");
                else
                    System.PrivateMessages[pe.ToPlayer].Add($"{pe.FromPlayer}: {pe.Message}");
            }
        }

        private void HandleChannelMessageEvent()
        {
            if (System.Queuer.NewChannelMessages.TryDequeue(out var ce))
            {
                if (!System.ChannelMessages.ContainsKey(ce.Channel))
                    System.ChannelMessages.Add(ce.Channel, new List<string>());
                //Highlight if the Channel isn't Selected.
                if (System.SelectedChannel != null && ce.Channel == "" &&
                    ce.FromPlayer != SettingsSystem.ServerSettings.ConsoleIdentifier)
                    if (!System.HighlightChannel.Contains(ce.Channel))
                        System.HighlightChannel.Add(ce.Channel);
                if (ce.Channel != System.SelectedChannel && ce.Channel != "")
                    if (!System.HighlightChannel.Contains(ce.Channel))
                        System.HighlightChannel.Add(ce.Channel);
                //Move the bar to the bottom on a new Message
                if (System.SelectedChannel == null && System.SelectedPmChannel == null && ce.Channel == "")
                {
                    Screen.ChatScrollPos.y = float.PositiveInfinity;
                    if (System.ChatLocked)
                        System.SelectTextBox = true;
                }
                if (System.SelectedChannel != null && System.SelectedPmChannel == null &&
                    ce.Channel == System.SelectedChannel)
                {
                    Screen.ChatScrollPos.y = float.PositiveInfinity;
                    if (System.ChatLocked)
                        System.SelectTextBox = true;
                }
                System.ChannelMessages[ce.Channel].Add($"{ce.FromPlayer}: {ce.Message}");
            }
        }

        private void HandlePlayerLeaveEvent()
        {
            if (System.Queuer.NewLeaveMessages.TryDequeue(out var jlm) &&
    System.PlayerChannels.ContainsKey(jlm.FromPlayer))
            {
                if (System.PlayerChannels[jlm.FromPlayer].Contains(jlm.Channel))
                    System.PlayerChannels[jlm.FromPlayer].Remove(jlm.Channel);
                if (System.PlayerChannels[jlm.FromPlayer].Count == 0)
                    System.PlayerChannels.Remove(jlm.FromPlayer);
            }
        }

        private void HandlePlayerJoinEvent()
        {
            if (System.Queuer.NewJoinMessages.TryDequeue(out var jlm))
            {
                if (!System.PlayerChannels.ContainsKey(jlm.FromPlayer))
                    System.PlayerChannels.Add(jlm.FromPlayer, new List<string>());
                if (!System.PlayerChannels[jlm.FromPlayer].Contains(jlm.Channel))
                    System.PlayerChannels[jlm.FromPlayer].Add(jlm.Channel);
            }
        }

        private void HandleLeaveEvent()
        {
            if (System.SelectedChannel != null && System.SelectedChannel != SettingsSystem.ServerSettings.ConsoleIdentifier)
            {
                System.MessageSender.SendMessage(new ChatLeaveMsgData
                {
                    From = SettingsSystem.CurrentSettings.PlayerName,
                    Channel = System.SelectedChannel
                });
                if (System.JoinedChannels.Contains(System.SelectedChannel))
                    System.JoinedChannels.Remove(System.SelectedChannel);
                System.SelectedChannel = null;
                System.SelectedPmChannel = null;
            }
            if (System.SelectedPmChannel != null)
            {
                if (System.JoinedPmChannels.Contains(System.SelectedPmChannel))
                    System.JoinedPmChannels.Remove(System.SelectedPmChannel);
                System.SelectedChannel = null;
                System.SelectedPmChannel = null;
            }
            System.LeaveEventHandled = true;
        }

        #endregion
    }
}