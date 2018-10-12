using LmpClient.Base;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.Warp;
using LmpClient.Utilities;
using LmpCommon.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace LmpClient.Windows.Status
{
    public partial class StatusWindow : Window<StatusWindow>
    {
        #region Fields

        #region Public

        public override bool Display => SettingsSystem.CurrentSettings.DisclaimerAccepted && MainSystem.ToolbarShowGui && 
                                        MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;

        public bool ColorEventHandled { get; set; } = true;

        #endregion

        private static Vector2 _scrollPosition;
        private static GUIStyle _subspaceStyle;
        private static GUIStyle _subspaceListStyle;
        
        private static Dictionary<string, GUIStyle> _playerNameStyle;
        private static GUIStyle _stateTextStyle;
        
        private static GUIStyle _highlightStyle;
        
        private const float WindowHeight = 400;
        private const float WindowWidth = 300;
        private const float UpdateStatusInterval = 1f;

        private static double _lastStatusUpdate;

        private static readonly List<SubspaceDisplayEntry> SubspaceDisplay = new List<SubspaceDisplayEntry>();

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

        protected override void DrawGui()
        {
            if (!ColorEventHandled)
            {
                _playerNameStyle = new Dictionary<string, GUIStyle>();
                ColorEventHandled = true;
            }

            //Calculate the minimum size of the minimize window by drawing it off the screen
            WindowRect = FixWindowPos(GUILayout.Window(6703 + MainSystem.WindowOffset,
                WindowRect, DrawContent, Title, LayoutOptions));
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width * 0.9f - WindowWidth, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, int.MaxValue, TitleHeight);
            
            _highlightStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = XKCDColors.Red },
                active = { textColor = XKCDColors.Red },
                hover = { textColor = XKCDColors.KSPNotSoGoodOrange }
            };

            _subspaceStyle = new GUIStyle(skin.box)
            {
                padding = new RectOffset(2, 2, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };

            _subspaceListStyle = new GUIStyle(skin.scrollView)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);
            
            _playerNameStyle = new Dictionary<string, GUIStyle>();

            _stateTextStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = XKCDColors.KSPNeutralUIGrey }};
            _stateTextStyle.hover.textColor = _stateTextStyle.normal.textColor;
            _stateTextStyle.active.textColor = _stateTextStyle.normal.textColor;
            _stateTextStyle.fontStyle = FontStyle.Normal;
            _stateTextStyle.fontSize = 12;
            _stateTextStyle.stretchWidth = true;
        }

        public override void Update()
        {
            base.Update();
            if (!Display) return;

            if (Time.realtimeSinceStartup - _lastStatusUpdate > UpdateStatusInterval)
            {
                _lastStatusUpdate = Time.realtimeSinceStartup;
                SubspaceDisplay.Clear();
                SubspaceDisplay.AddRange(WarpSystem.Singleton.WarpEntryDisplay.GetSubspaceDisplayEntries());
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
