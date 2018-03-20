using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.Chat;
using LunaClient.Utilities;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Windows.Chat
{
    public partial class ChatWindow : Window<ChatWindow>
    {
        #region Fields

        private static bool _display;
        public override bool Display
        {
            get => _display && MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => _display = value;
        }

        public string ChatWindowLock { get; set; } = "LMP_Chat_Window_Lock";

        public float WindowHeight { get; set; } = 300;
        public float WindowWidth { get; set; } = 400;

        #region Layout

        protected static GUILayoutOption[] WindowLayoutOptions { get; set; }

        private Vector2 _chatScrollPos;

        #endregion

        #endregion

        #region Base overrides

        protected override bool Resizable => true;

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
            
            _chatScrollPos = new Vector2(0, 0);
            HighlightStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = Color.red },
                active = { textColor = Color.red },
                hover = { textColor = Color.red }
            };
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
            base.Update();
            Display &= MainSystem.NetworkState >= ClientState.Running;
            SafeDisplay = Display;
            
            if (Display)
            {
                if (ChatSystem.Singleton.NewMessageReceived)
                    ChatSystem.Singleton.NewMessageReceived = false;
            }
        }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
            {
                WindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6704 + MainSystem.WindowOffset, WindowRect, DrawContent, LocalizationContainer.ChatWindowText.Title, WindowStyle));
            }
            CheckWindowLock();
        }

        #endregion

        #region Public methods

        public void ScrollToBottom()
        {
            _chatScrollPos.y = float.PositiveInfinity;
        }

        #endregion

        #region Private methods

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

        #endregion
    }
}
