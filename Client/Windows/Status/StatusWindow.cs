using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Windows.Status
{
    public partial class StatusWindow : Window<StatusWindow>
    {
        #region Fields

        #region Public

        public SubspaceDisplayEntry[] SubspaceDisplay { get; set; }
        public bool DisconnectEventHandled { get; set; } = true;
        public bool ColorEventHandled { get; set; } = true;

        #endregion

        protected bool Minmized { get; set; }
        protected Vector2 ScrollPosition { get; set; }
        protected bool SafeMinimized { get; set; }
        protected GUIStyle SubspaceStyle { get; set; }
        protected Dictionary<string, GUIStyle> PlayerNameStyle { get; set; }
        protected GUIStyle VesselNameStyle { get; set; }
        protected GUIStyle StateTextStyle { get; set; }

        protected const float WindowHeight = 400;
        protected const float WindowWidth = 300;
        private const float UpdateStatusInterval = .2f;

        private double LastStatusUpdate { get; set; }
        private bool CalculatedMinSize { get; set; }

        #endregion

        public override void OnGui()
        {
            if (!ColorEventHandled)
            {
                PlayerNameStyle = new Dictionary<string, GUIStyle>();
                ColorEventHandled = true;
            }
            base.OnGui();
            if (Display)
            {
                //Calculate the minimum size of the minimize window by drawing it off the screen
                if (!CalculatedMinSize)
                    MinWindowRect = GUILayout.Window(6701 + MainSystem.WindowOffset, MinWindowRect, DrawMaximize,
                        "LMP", WindowStyle, MinLayoutOptions);
                if (!SafeMinimized)
                    WindowRect =
                        LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6703 + MainSystem.WindowOffset, WindowRect,
                            DrawContent,
                            "LunaMultiPlayer - Status", WindowStyle, LayoutOptions));
                else
                    MinWindowRect =
                        LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6703 + MainSystem.WindowOffset, MinWindowRect,
                            DrawMaximize, "LMP", WindowStyle, MinLayoutOptions));
            }
            CheckWindowLock();
        }

        public override void Reset()
        {
            base.Reset();
            Display = false;
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width*0.9f - WindowWidth, Screen.height/2f - WindowHeight/2f, WindowWidth,
                WindowHeight);
            MinWindowRect = new Rect(float.NegativeInfinity, float.NegativeInfinity, 0, 0);
            MoveRect = new Rect(0, 0, 10000, 20);

            WindowStyle = new GUIStyle(GUI.skin.window);
            ButtonStyle = new GUIStyle(GUI.skin.button);
            HighlightStyle = new GUIStyle(GUI.skin.button)
            {
                normal = {textColor = Color.red},
                active = {textColor = Color.red},
                hover = {textColor = Color.red}
            };
            ScrollStyle = new GUIStyle(GUI.skin.scrollView);
            SubspaceStyle = new GUIStyle {normal = {background = new Texture2D(1, 1)}};
            SubspaceStyle.normal.background.SetPixel(0, 0, Color.black);
            SubspaceStyle.normal.background.Apply();

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            MinLayoutOptions = new GUILayoutOption[4];
            MinLayoutOptions[0] = GUILayout.MinWidth(0);
            MinLayoutOptions[1] = GUILayout.MinHeight(0);
            MinLayoutOptions[2] = GUILayout.ExpandHeight(true);
            MinLayoutOptions[3] = GUILayout.ExpandWidth(true);

            //Adapted from KMP.
            PlayerNameStyle = new Dictionary<string, GUIStyle>();

            VesselNameStyle = new GUIStyle(GUI.skin.label) {normal = {textColor = Color.white}};
            VesselNameStyle.hover.textColor = VesselNameStyle.normal.textColor;
            VesselNameStyle.active.textColor = VesselNameStyle.normal.textColor;
            VesselNameStyle.fontStyle = FontStyle.Normal;
            VesselNameStyle.fontSize = 12;
            VesselNameStyle.stretchWidth = true;

            StateTextStyle = new GUIStyle(GUI.skin.label) {normal = {textColor = new Color(0.75f, 0.75f, 0.75f)}};
            StateTextStyle.hover.textColor = StateTextStyle.normal.textColor;
            StateTextStyle.active.textColor = StateTextStyle.normal.textColor;
            StateTextStyle.fontStyle = FontStyle.Normal;
            StateTextStyle.fontSize = 12;
            StateTextStyle.stretchWidth = true;

            SubspaceDisplay = new SubspaceDisplayEntry[0];
        }

        public override void Update()
        {
            Display = MainSystem.Singleton.GameRunning;
            if (Display)
            {
                SafeMinimized = Minmized;
                if (!CalculatedMinSize && (MinWindowRect.width != 0) && (MinWindowRect.height != 0))
                    CalculatedMinSize = true;
                if (Time.realtimeSinceStartup - LastStatusUpdate > UpdateStatusInterval)
                {
                    LastStatusUpdate = Time.realtimeSinceStartup;
                    SubspaceDisplay = WarpSystem.Singleton.WarpEntryDisplay.GetSubspaceDisplayEntries();
                }
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
            if (!MainSystem.Singleton.GameRunning)
            {
                RemoveWindowLock();
                return;
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                RemoveWindowLock();
                return;
            }

            if (Display)
            {
                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                var shouldLock = Minmized ? MinWindowRect.Contains(mousePos) : WindowRect.Contains(mousePos);

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