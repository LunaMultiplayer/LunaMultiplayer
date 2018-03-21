using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaCommon.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Windows.Status
{
    public partial class StatusWindow : Window<StatusWindow>
    {
        #region Fields

        #region Public

        public override bool Display => SettingsSystem.CurrentSettings.DisclaimerAccepted && MainSystem.ToolbarShowGui && 
                                        MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;

        public List<SubspaceDisplayEntry> SubspaceDisplay { get; set; }
        public bool DisconnectEventHandled { get; set; } = true;
        public bool ColorEventHandled { get; set; } = true;

        #endregion

        private static Vector2 _scrollPosition;
        private static GUIStyle _subspaceStyle;
        private static Dictionary<string, GUIStyle> _playerNameStyle;
        private static GUIStyle _vesselNameStyle;
        private static GUIStyle _stateTextStyle;

        private const float WindowHeight = 400;
        private const float WindowWidth = 300;
        private const float UpdateStatusInterval = .5f;

        private static double _lastStatusUpdate;

        private static Texture2D _chatIcon;
        private static Texture2D _chatRedIcon;
        private static Texture2D _cameraIcon;
        private static Texture2D _rocketIcon;

#if DEBUG
        private static readonly string Title = $"LMP - Debug port: {CommonUtil.DebugPort}";
#else
        private static readonly string Title = $"LMP - Luna Multiplayer";
#endif

        #endregion

        protected override void OnCloseButton()
        {
            base.OnCloseButton();
            RemoveWindowLock();
            MainSystem.ToolbarShowGui = false;
        }

        public override void OnGui()
        {
            base.OnGui();
            
            if (Display)
            {
                if (!ColorEventHandled)
                {
                    _playerNameStyle = new Dictionary<string, GUIStyle>();
                    ColorEventHandled = true;
                }

                //Calculate the minimum size of the minimize window by drawing it off the screen
                WindowRect = FixWindowPos(GUILayout.Window(6703 + MainSystem.WindowOffset, 
                    WindowRect, DrawContent, Title, WindowStyle, LayoutOptions));
            }
            CheckWindowLock();
        }

        public override void SetStyles()
        {
            _chatIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "chatWhite.png"), 16, 16);
            _chatRedIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "chatRed.png"), 16, 16);
            _cameraIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "camera.png"), 16, 16);
            _rocketIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "rocket.png"), 16, 16);

            WindowRect = new Rect(Screen.width * 0.9f - WindowWidth, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
            HighlightStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = Color.red },
                active = { textColor = Color.red },
                hover = { textColor = Color.red }
            };
            _subspaceStyle = new GUIStyle { normal = { background = new Texture2D(1, 1) } };
            _subspaceStyle.normal.background.SetPixel(0, 0, Color.black);
            _subspaceStyle.normal.background.Apply();

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);
            
            _playerNameStyle = new Dictionary<string, GUIStyle>();

            _vesselNameStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.white } };
            _vesselNameStyle.hover.textColor = _vesselNameStyle.normal.textColor;
            _vesselNameStyle.active.textColor = _vesselNameStyle.normal.textColor;
            _vesselNameStyle.fontStyle = FontStyle.Normal;
            _vesselNameStyle.fontSize = 12;
            _vesselNameStyle.stretchWidth = true;

            _stateTextStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = new Color(0.75f, 0.75f, 0.75f) } };
            _stateTextStyle.hover.textColor = _stateTextStyle.normal.textColor;
            _stateTextStyle.active.textColor = _stateTextStyle.normal.textColor;
            _stateTextStyle.fontStyle = FontStyle.Normal;
            _stateTextStyle.fontSize = 12;
            _stateTextStyle.stretchWidth = true;

            SubspaceDisplay = new List<SubspaceDisplayEntry>();
        }

        public override void Update()
        {
            base.Update();
            if (!Display) return;

            if (Time.realtimeSinceStartup - _lastStatusUpdate > UpdateStatusInterval)
            {
                _lastStatusUpdate = Time.realtimeSinceStartup;
                SubspaceDisplay = WarpSystem.Singleton.WarpEntryDisplay.GetSubspaceDisplayEntries();
            }
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_PlayerStatusLock");
            }
        }

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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_PlayerStatusLock");
                    IsWindowLocked = true;
                }
                if (!shouldLock && IsWindowLocked)
                    RemoveWindowLock();
            }

            if (!Display && IsWindowLocked)
                RemoveWindowLock();
        }
    }
}
