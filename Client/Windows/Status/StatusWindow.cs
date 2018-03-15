using LunaClient.Base;
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

        public override bool Display => MainSystem.ToolbarShowGui && MainSystem.NetworkState >= ClientState.Running &&
                                        HighLogic.LoadedScene >= GameScenes.SPACECENTER;

        public List<SubspaceDisplayEntry> SubspaceDisplay { get; set; }
        public bool DisconnectEventHandled { get; set; } = true;
        public bool ColorEventHandled { get; set; } = true;

        #endregion
        
        protected Vector2 ScrollPosition { get; set; }
        protected GUIStyle SubspaceStyle { get; set; }
        protected Dictionary<string, GUIStyle> PlayerNameStyle { get; set; }
        protected GUIStyle VesselNameStyle { get; set; }
        protected GUIStyle StateTextStyle { get; set; }

        protected const float WindowHeight = 400;
        protected const float WindowWidth = 300;
        private const float UpdateStatusInterval = .5f;

        private double LastStatusUpdate { get; set; }

#if DEBUG
        private readonly string _title = $"LMP - Debug port: {CommonUtil.DebugPort}";
#else
        private readonly string _title = $"LMP - Luna Multiplayer";
#endif

        #endregion

        public override void OnGui()
        {
            base.OnGui();
            
            if (Display)
            {
                if (!ColorEventHandled)
                {
                    PlayerNameStyle = new Dictionary<string, GUIStyle>();
                    ColorEventHandled = true;
                }

                //Calculate the minimum size of the minimize window by drawing it off the screen
                WindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6703 + MainSystem.WindowOffset, 
                    WindowRect, DrawContent, _title, WindowStyle, LayoutOptions));
            }
            CheckWindowLock();
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width * 0.9f - WindowWidth, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
            HighlightStyle = new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = Color.red },
                active = { textColor = Color.red },
                hover = { textColor = Color.red }
            };
            SubspaceStyle = new GUIStyle { normal = { background = new Texture2D(1, 1) } };
            SubspaceStyle.normal.background.SetPixel(0, 0, Color.black);
            SubspaceStyle.normal.background.Apply();

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);
            
            PlayerNameStyle = new Dictionary<string, GUIStyle>();

            VesselNameStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.white } };
            VesselNameStyle.hover.textColor = VesselNameStyle.normal.textColor;
            VesselNameStyle.active.textColor = VesselNameStyle.normal.textColor;
            VesselNameStyle.fontStyle = FontStyle.Normal;
            VesselNameStyle.fontSize = 12;
            VesselNameStyle.stretchWidth = true;

            StateTextStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = new Color(0.75f, 0.75f, 0.75f) } };
            StateTextStyle.hover.textColor = StateTextStyle.normal.textColor;
            StateTextStyle.active.textColor = StateTextStyle.normal.textColor;
            StateTextStyle.fontStyle = FontStyle.Normal;
            StateTextStyle.fontSize = 12;
            StateTextStyle.stretchWidth = true;

            SubspaceDisplay = new List<SubspaceDisplayEntry>();
        }

        public override void Update()
        {
            if (!Display) return;

            if (Time.realtimeSinceStartup - LastStatusUpdate > UpdateStatusInterval)
            {
                LastStatusUpdate = Time.realtimeSinceStartup;
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