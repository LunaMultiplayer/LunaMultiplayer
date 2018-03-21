using LunaClient.Base;
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
            get => base.Display && _display && MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => base.Display = _display = value;
        }

        private const float WindowHeight = 400;
        private const float WindowWidth = 400;
        
        #region Vessel systems

        private static bool _vesselDock;
        private static bool _vesselFlightState;
        private static bool _vesselImmortal;
        private static bool _vesselLock;
        private static bool _vesselPosition;
        private static bool _vesselUpdate;
        private static bool _vesselPartSync;
        private static bool _vesselResource;
        private static bool _vesselFairing;
        private static bool _vesselProto;
        private static bool _vesselPrecalc;
        private static bool _vesselState;
        private static bool _vesselRemove;
        private static bool _vesselSwitcher;
        #endregion

        #region Other systems

        private static bool _asteroid;
        private static bool _craftLibrary;
        private static bool _facility;
        private static bool _flag;
        private static bool _kscScene;
        private static bool _group;
        private static bool _kerbal;
        private static bool _lock;
        private static bool _modS;
        private static bool _modApi;
        private static bool _playerColor;
        private static bool _playerConnection;
        private static bool _scenario;
        private static bool _timeSyncer;
        private static bool _toolbar;
        private static bool _warp;

        #endregion

        #endregion
        
        public override void OnGui()
        {
            base.OnGui();
            if (Display)
            {
                WindowRect = FixWindowPos(GUILayout.Window(6716 + MainSystem.WindowOffset, WindowRect, DrawContent, "Systems", WindowStyle, LayoutOptions));
            }
            CheckWindowLock();
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width - (WindowWidth + 50), Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            TextAreaOptions = new GUILayoutOption[1];
            TextAreaOptions[0] = GUILayout.ExpandWidth(true);
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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_SystemsWindowsLock");
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
