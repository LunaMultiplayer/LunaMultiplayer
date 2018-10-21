using LmpClient.Localization;
using LmpClient.Systems.Chat;
using LmpClient.Systems.PlayerColorSys;
using UnityEngine;

namespace LmpClient.Windows.Chat
{
    public partial class ChatWindow
    {
        private static string _chatInputText = string.Empty;

        protected override void DrawWindowContent(int windowId)
        {
            var pressedEnter = Event.current.type == EventType.KeyDown && !Event.current.shift && Event.current.character == '\n';
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            DrawChatMessageBox();
            DrawTextInput(pressedEnter);
            GUILayout.Space(5);
            GUILayout.EndVertical();
        }

        private static void DrawChatMessageBox()
        {
            _chatScrollPos = GUILayout.BeginScrollView(_chatScrollPos);
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            foreach (var channelMessageTuple in ChatSystem.Singleton.ChatMessages)
            {
                var playerName = channelMessageTuple.Item1;
                var chatMessage = channelMessageTuple.Item2;

                _playerNameStyle.normal.textColor = PlayerColorSystem.Singleton.GetPlayerColor(playerName);

                GUILayout.BeginHorizontal();
                GUILayout.Label(playerName, _playerNameStyle);
                // Make a separate Label for the ": " because it gives weird word wrapping if it's together with the chatMessage
                GUILayout.Label(": ", _chatMessageStyle);
                GUILayout.Label(chatMessage, _chatMessageStyle);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
        
        private static void DrawTextInput(bool pressedEnter)
        {
            GUILayout.BeginHorizontal();
            
            if (pressedEnter || GUILayout.Button(LocalizationContainer.ChatWindowText.Send, GUILayout.Width(WindowWidth * .25f)))
            {
                if (!string.IsNullOrEmpty(_chatInputText))
                {
                    ChatSystem.Singleton.MessageSender.SendChatMsg(_chatInputText.Trim('\n'));
                }

                _chatInputText = string.Empty;
            }
            else
            {
                _chatInputText = GUILayout.TextArea(_chatInputText);
            }

            GUILayout.EndHorizontal();
        }
    }
}