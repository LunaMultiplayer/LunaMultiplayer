using LunaClient.Base;
using LunaClient.Utilities;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Windows.Systems
{
    /// <summary>
    /// Here we should only display the statistics for systems that contain ROUTINES or code that executes on every fixedupdate/update/lateupdate
    /// </summary>
    public partial class SystemsWindow : Window<SystemsWindow>
    {
        #region Fields & properties

        private static bool _display;
        public override bool Display
        {
            get
            {
                if (!_display) return false;

                return MainSystem.NetworkState >= ClientState.Running &&
                    HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            } 
            set => _display = value;
        }

        private float WindowHeight { get; } = 400;
        private float WindowWidth { get; } = 400;
        
        #region Vessel systems

        private bool VesselDock { get; set; }

        private bool VesselFlightState { get; set; }

        private bool VesselImmortal { get; set; }

        private bool VesselLock { get; set; }

        private bool VesselPosition { get; set; }

        private bool VesselUpdate { get; set; }

        private bool VesselProto { get; set; }

        private bool VesselRange { get; set; }

        private bool VesselState { get; set; }

        private bool VesselRemove { get; set; }

        private bool VesselSwitcher { get; set; }

        #endregion

        #region Other systems

        private bool Asteroid { get; set; }

        private bool CraftLibrary { get; set; }

        private bool Facility { get; set; }

        private bool Flag { get; set; }

        private bool GameScene { get; set; }

        private bool Group { get; set; }

        private bool Kerbal { get; set; }

        private bool Lock { get; set; }

        private bool ModS { get; set; }

        private bool ModApi { get; set; }

        private bool PlayerColor { get; set; }

        private bool PlayerConnection { get; set; }

        private bool Scenario { get; set; }

        private bool TimeSyncer { get; set; }

        private bool Toolbar { get; set; }

        private bool Warp { get; set; }

        #endregion

        #endregion

        public override void Update()
        {
            SafeDisplay = Display;
        }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
                WindowRect =
                    LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6715 + MainSystem.WindowOffset, WindowRect,
                        DrawContent, "LunaMultiPlayer - Systems", WindowStyle, LayoutOptions));
            CheckWindowLock();
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width - (WindowWidth + 50), Screen.height / 2f - WindowHeight / 2f, WindowWidth,
                WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            WindowStyle = new GUIStyle(GUI.skin.window);
            ButtonStyle = new GUIStyle(GUI.skin.button);

            TextAreaOptions = new GUILayoutOption[1];
            TextAreaOptions[0] = GUILayout.ExpandWidth(true);

            LabelStyle = new GUIStyle(GUI.skin.label);
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_SystemsWindowsLock");
            }
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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_SystemsWindowsLock");
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
