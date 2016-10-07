using System.Collections.Generic;
using LunaClient.Base.Interface;
using LunaClient.Systems.Admin;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Windows.Chat
{
    public partial class ChatWindow
    {
        public void DrawContent(int windowId)
        {
            var pressedEnter = (Event.current.type == EventType.KeyDown) && !Event.current.shift &&
                               (Event.current.character == '\n');
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GUILayout.BeginHorizontal();
            DrawRooms();
            GUILayout.FlexibleSpace();
            if (((System.SelectedChannel != null) && (System.SelectedChannel != SettingsSystem.ServerSettings.ConsoleIdentifier)) ||
                (System.SelectedPmChannel != null))
                if (GUILayout.Button("Leave", ButtonStyle))
                    System.LeaveEventHandled = false;
            DrawConsole();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ChatScrollPos = GUILayout.BeginScrollView(ChatScrollPos, ScrollStyle);
            if ((System.SelectedChannel == null) && (System.SelectedPmChannel == null))
            {
                if (!System.ChannelMessages.ContainsKey(""))
                    System.ChannelMessages.Add("", new List<string>());
                foreach (var channelMessage in System.ChannelMessages[""])
                    GUILayout.Label(channelMessage, LabelStyle);
            }
            if ((System.SelectedChannel != null) && (System.SelectedChannel != SettingsSystem.ServerSettings.ConsoleIdentifier))
            {
                if (!System.ChannelMessages.ContainsKey(System.SelectedChannel))
                    System.ChannelMessages.Add(System.SelectedChannel, new List<string>());
                foreach (var channelMessage in System.ChannelMessages[System.SelectedChannel])
                    GUILayout.Label(channelMessage, LabelStyle);
            }
            if (System.SelectedChannel == SettingsSystem.ServerSettings.ConsoleIdentifier)
                foreach (var consoleMessage in System.ConsoleMessages)
                    GUILayout.Label(consoleMessage, LabelStyle);
            if (System.SelectedPmChannel != null)
            {
                if (!System.PrivateMessages.ContainsKey(System.SelectedPmChannel))
                    System.PrivateMessages.Add(System.SelectedPmChannel, new List<string>());
                foreach (var privateMessage in System.PrivateMessages[System.SelectedPmChannel])
                    GUILayout.Label(privateMessage, LabelStyle);
            }
            GUILayout.EndScrollView();
            PlayerScrollPos = GUILayout.BeginScrollView(PlayerScrollPos, ScrollStyle, SmallSizeOption);
            GUILayout.BeginVertical();
            GUILayout.Label(SettingsSystem.CurrentSettings.PlayerName, LabelStyle);
            if (System.SelectedPmChannel != null)
            {
                GUILayout.Label(System.SelectedPmChannel, LabelStyle);
            }
            else
            {
                if (System.SelectedChannel == null)
                    foreach (var player in StatusSystem.Singleton.PlayerStatusList.Values)
                    {
                        if (System.JoinedPmChannels.Contains(player.PlayerName))
                            GUI.enabled = false;
                        if (GUILayout.Button(player.PlayerName, LabelStyle))
                            if (!System.JoinedPmChannels.Contains(player.PlayerName))
                                System.JoinedPmChannels.Add(player.PlayerName);
                        GUI.enabled = true;
                    }
                else
                    foreach (var playerEntry in System.PlayerChannels)
                        if ((playerEntry.Key != SettingsSystem.CurrentSettings.PlayerName) &&
                            playerEntry.Value.Contains(System.SelectedChannel))
                        {
                            if (System.JoinedPmChannels.Contains(playerEntry.Key))
                                GUI.enabled = false;
                            if (GUILayout.Button(playerEntry.Key, LabelStyle))
                                if (!System.JoinedPmChannels.Contains(playerEntry.Key))
                                    System.JoinedPmChannels.Add(playerEntry.Key);
                            GUI.enabled = true;
                        }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("ChatBase.SendTextArea");
            var tempSendText = GUILayout.TextArea(System.SendText, TextAreaStyle);
            //When a control is inserted or removed from the GUI, Unity's focusing starts tripping balls. This is a horrible workaround for unity that shouldn't exist...
            var newTextId = GUIUtility.GetControlID(FocusType.Keyboard);
            if (PreviousTextId != newTextId)
            {
                PreviousTextId = newTextId;
                if (System.ChatLocked)
                    System.SelectTextBox = true;
            }
            //Don't add the newline to the messages, queue a send
            if (!IgnoreChatInput)
                if (pressedEnter)
                    System.SendEventHandled = false;
                else
                    System.SendText = tempSendText;
            if (System.SendText == "")
                GUI.enabled = false;
            if (GUILayout.Button("Send", ButtonStyle, SmallSizeOption))
                System.SendEventHandled = false;
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            if (!System.SelectTextBox)
            {
                if ((GUI.GetNameOfFocusedControl() == "ChatBase.SendTextArea") && !System.ChatLocked)
                {
                    System.ChatLocked = true;
                    InputLockManager.SetControlLock(LmpGuiUtil.BlockAllControls, System.LmpChatLock);
                }
                if ((GUI.GetNameOfFocusedControl() != "ChatBase.SendTextArea") && System.ChatLocked)
                {
                    System.ChatLocked = false;
                    InputLockManager.RemoveControlLock(System.LmpChatLock);
                }
            }
            else
            {
                System.SelectTextBox = false;
                GUI.FocusControl("ChatBase.SendTextArea");
            }
        }

        private void DrawConsole()
        {
            if (System.SelectedChannel == SettingsSystem.ServerSettings.ConsoleIdentifier)
                GUI.enabled = false;
            var possibleHighlightButtonStyle = System.HighlightChannel.Contains(SettingsSystem.ServerSettings.ConsoleIdentifier)
                ? HighlightStyle
                : ButtonStyle;
            if (AdminSystem.Singleton.IsCurrentPlayerAdmin() &&
                GUILayout.Button("#" + SettingsSystem.ServerSettings.ConsoleIdentifier, possibleHighlightButtonStyle))
            {
                if (System.HighlightChannel.Contains(SettingsSystem.ServerSettings.ConsoleIdentifier))
                    System.HighlightChannel.Remove(SettingsSystem.ServerSettings.ConsoleIdentifier);
                System.SelectedChannel = SettingsSystem.ServerSettings.ConsoleIdentifier;
                System.SelectedPmChannel = null;
                ChatScrollPos.y = float.PositiveInfinity;
            }
            GUI.enabled = true;
        }

        private void DrawRooms()
        {
            if ((System.SelectedChannel == null) && (System.SelectedPmChannel == null))
                GUI.enabled = false;
            var possibleHighlightButtonStyle = System.HighlightChannel.Contains("") ? HighlightStyle : ButtonStyle;
            if (GUILayout.Button("#Global", possibleHighlightButtonStyle))
            {
                if (System.HighlightChannel.Contains(""))
                    System.HighlightChannel.Remove("");
                System.SelectedChannel = null;
                System.SelectedPmChannel = null;
                ChatScrollPos.y = float.PositiveInfinity;
            }
            GUI.enabled = true;
            foreach (var joinedChannel in System.JoinedChannels)
            {
                possibleHighlightButtonStyle = System.HighlightChannel.Contains(joinedChannel)
                    ? HighlightStyle
                    : ButtonStyle;
                if (System.SelectedChannel == joinedChannel)
                    GUI.enabled = false;
                if (GUILayout.Button("#" + joinedChannel, possibleHighlightButtonStyle))
                {
                    if (System.HighlightChannel.Contains(joinedChannel))
                        System.HighlightChannel.Remove(joinedChannel);
                    System.SelectedChannel = joinedChannel;
                    System.SelectedPmChannel = null;
                    ChatScrollPos.y = float.PositiveInfinity;
                }
                GUI.enabled = true;
            }

            foreach (var joinedPlayer in System.JoinedPmChannels)
            {
                possibleHighlightButtonStyle = System.HighlightPm.Contains(joinedPlayer) ? HighlightStyle : ButtonStyle;
                if (System.SelectedPmChannel == joinedPlayer)
                    GUI.enabled = false;
                if (GUILayout.Button("@" + joinedPlayer, possibleHighlightButtonStyle))
                {
                    if (System.HighlightPm.Contains(joinedPlayer))
                        System.HighlightPm.Remove(joinedPlayer);
                    System.SelectedChannel = null;
                    System.SelectedPmChannel = joinedPlayer;
                    ChatScrollPos.y = float.PositiveInfinity;
                }
                GUI.enabled = true;
            }
        }
    }
}