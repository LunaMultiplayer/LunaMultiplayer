using LunaClient.Localization;
using LunaClient.Systems.Chat;
using UnityEngine;

namespace LunaClient.Windows.Chat
{
    public partial class ChatWindow
    {
        private static string _chatInputText = string.Empty;

        public void DrawContent(int windowId)
        {
            if (GUI.RepeatButton(new Rect(WindowRect.width - 18, 2, 16, 16), ResizeIcon))
            {
                _resizingWindow = true;
            }

            var pressedEnter = Event.current.type == EventType.KeyDown && !Event.current.shift && Event.current.character == '\n';
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            DrawChatMessageBox();
            DrawTextInput(pressedEnter);

            GUILayout.EndVertical();
        }

        private void DrawChatMessageBox()
        {
            GUILayout.BeginHorizontal();
            _chatScrollPos = GUILayout.BeginScrollView(_chatScrollPos, ScrollStyle);
            GUILayout.BeginVertical(BoxStyle);
            GUILayout.FlexibleSpace();
            foreach (var channelMessage in ChatSystem.Singleton.ChatMessages)
                GUILayout.Label(channelMessage, LabelStyle);
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }
        
        private void DrawTextInput(bool pressedEnter)
        {
            GUILayout.BeginHorizontal();
            _chatInputText = GUILayout.TextArea(_chatInputText, TextAreaStyle);
            if (pressedEnter || GUILayout.Button(LocalizationContainer.ChatWindowText.Send, ButtonStyle, GUILayout.Width(WindowWidth * .25f)))
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