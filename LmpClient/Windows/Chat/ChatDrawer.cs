using LmpClient.Localization;
using LmpClient.Systems.Chat;
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
            GUILayout.BeginHorizontal();
            _chatScrollPos = GUILayout.BeginScrollView(_chatScrollPos);
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            foreach (var channelMessage in ChatSystem.Singleton.ChatMessages)
                GUILayout.Label(channelMessage);
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }
        
        private static void DrawTextInput(bool pressedEnter)
        {
            GUILayout.BeginHorizontal();
            _chatInputText = GUILayout.TextArea(_chatInputText);
            if (pressedEnter || GUILayout.Button(LocalizationContainer.ChatWindowText.Send, GUILayout.Width(WindowWidth * .25f)))
            {
                if (!string.IsNullOrEmpty(_chatInputText))
                {
                    ChatSystem.Singleton.MessageSender.SendChatMsg(_chatInputText.Trim('\n'));
                }

                _chatInputText = string.Empty;
            }

            GUILayout.EndHorizontal();
        }
    }
}