using LunaClient.Base;
using LunaClient.Systems.Chat;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Windows.Chat
{
    public partial class ChatWindow : SystemWindow<ChatWindow, ChatSystem>
    {
        #region Fields

        private static bool _display;
        public override bool Display
        {
            get => _display && MainSystem.NetworkState >= ClientState.Running &&
                   HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => _display = value;
        }

        public string ChatWindowLock { get; set; } = "LMP_Chat_Window_Lock";
        public bool IgnoreChatInput { get; set; }
        public float WindowHeight { get; set; } = 300;
        public float WindowWidth { get; set; } = 400;
        protected int PreviousTextId { get; set; } = 0;

        #region Layout

        protected static GUILayoutOption[] WindowLayoutOptions { get; set; }
        protected static GUILayoutOption[] SmallSizeOption { get; set; }

        protected static Vector2 PlayerScrollPos { get; set; }
        public Vector2 ChatScrollPos;

        #endregion

        #endregion

        public override void SetStyles()
        {
            // ReSharper disable once PossibleLossOfFraction
            WindowRect = new Rect(Screen.width / 10, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);

            WindowLayoutOptions = new GUILayoutOption[4];
            WindowLayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            WindowLayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            WindowLayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            WindowLayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            SmallSizeOption = new GUILayoutOption[1];
            SmallSizeOption[0] = GUILayout.Width(WindowWidth * .25f);

            WindowStyle = new GUIStyle(GUI.skin.window);
            ScrollStyle = new GUIStyle(GUI.skin.scrollView);

            ChatScrollPos = new Vector2(0, 0);
            LabelStyle = new GUIStyle(GUI.skin.label);
            ButtonStyle = new GUIStyle(GUI.skin.button);
            HighlightStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = Color.red },
                active = { textColor = Color.red },
                hover = { textColor = Color.red }
            };
            TextAreaStyle = new GUIStyle(GUI.skin.textArea);
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock(ChatWindowLock);
            }
        }

        public override void Update()
        {
            Display &= MainSystem.NetworkState >= ClientState.Running;
            SafeDisplay = Display;
            IgnoreChatInput = false;
            if (System.ChatButtonHighlighted && Display)
                System.ChatButtonHighlighted = false;
            if (System.ChatLocked && !Display)
            {
                System.ChatLocked = false;
                InputLockManager.RemoveControlLock(System.LmpChatLock);
            }
        }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
            {
                var pressedChatShortcutKey = Event.current.type == EventType.KeyDown &&
                                             Event.current.keyCode == SettingsSystem.CurrentSettings.ChatKey;
                if (pressedChatShortcutKey)
                {
                    IgnoreChatInput = true;
                    System.SelectTextBox = true;
                }
                WindowRect =
                    LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6704 + MainSystem.WindowOffset, WindowRect,
                        DrawContent, "LunaMultiPlayer Chat", WindowStyle, WindowLayoutOptions));
            }
            CheckWindowLock();
        }


        public void SizeChanged()
        {
            Initialized = false;
            System.PrintToSelectedChannel($"New window size is: {WindowWidth}x{WindowHeight}");
        }

        private void CheckWindowLock()
        {
            if (SafeDisplay)
            {
                if (MainSystem.NetworkState < ClientState.Running || HighLogic.LoadedSceneIsFlight)
                {
                    RemoveWindowLock();
                    return;
                }

                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                var shouldLock = WindowRect.Contains(mousePos);

                if (shouldLock && !IsWindowLocked)
                {
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, ChatWindowLock);
                    IsWindowLocked = true;
                }
                if (!shouldLock && IsWindowLocked)
                    RemoveWindowLock();
            }

            if (!SafeDisplay && IsWindowLocked)
                RemoveWindowLock();
        }
    }
}