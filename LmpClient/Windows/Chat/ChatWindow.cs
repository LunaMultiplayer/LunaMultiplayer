using LmpClient.Base;
using LmpClient.Localization;
using LmpClient.Systems.Chat;
using LmpCommon.Enums;
using UnityEngine;

namespace LmpClient.Windows.Chat
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

        private const float WindowHeight = 300;
        private const float WindowWidth = 400;

        private static Vector2 _chatScrollPos;

        private static GUIStyle _playerNameStyle;
        private static GUIStyle _chatMessageStyle;

        #endregion

        #region Base overrides

        protected override bool Resizable => true;

        public override void SetStyles()
        {
            // ReSharper disable once PossibleLossOfFraction
            WindowRect = new Rect(Screen.width / 10, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, int.MaxValue, TitleHeight);

            _chatScrollPos = new Vector2(0, 0);
            _playerNameStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Normal,
                stretchWidth = false,
                wordWrap = false
            };

            _chatMessageStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = XKCDColors.OffWhite },
                fontStyle = FontStyle.Normal,
                wordWrap = true,
                stretchWidth = false
            };
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_ChatLock");
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

        protected override void DrawGui()
        {
            WindowRect = FixWindowPos(GUILayout.Window(6704 + MainSystem.WindowOffset, WindowRect, DrawContent,
                LocalizationContainer.ChatWindowText.Title));
        }

        #endregion

        #region Public methods

        public void ScrollToBottom()
        {
            _chatScrollPos.y = float.PositiveInfinity;
        }

        #endregion

        #region Private methods

        public override void CheckWindowLock()
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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_ChatLock");
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
