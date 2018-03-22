using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.Chat;
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
            get => base.Display && _display && MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => base.Display = _display = value;
        }

        private const string ChatWindowLock = "LMP_Chat_Window_Lock";
        private const float WindowHeight = 300;
        private const float WindowWidth = 400;
        
        private static Vector2 _chatScrollPos;

        #endregion

        #region Base overrides

        protected override bool Resizable => true;

        public override void SetStyles()
        {
            // ReSharper disable once PossibleLossOfFraction
            WindowRect = new Rect(Screen.width / 10, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
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
            if (Display)
            {
                if (ChatSystem.Singleton.NewMessageReceived)
                    ChatSystem.Singleton.NewMessageReceived = false;
            }
        }

        public override void OnGui()
        {
            base.OnGui();
            if (Display)
            {
                WindowRect = FixWindowPos(GUILayout.Window(6704 + MainSystem.WindowOffset, WindowRect, DrawContent, 
                    LocalizationContainer.ChatWindowText.Title, WindowStyle));
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
            if (Display)
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

            if (!Display && IsWindowLocked)
                RemoveWindowLock();
        }

        #endregion
    }
}
