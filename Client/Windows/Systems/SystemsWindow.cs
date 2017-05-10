using LunaClient.Base;
using LunaClient.Systems.VesselChangeSys;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselImmortalSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRangeSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Windows.Systems
{
    public partial class SystemsWindow : Window<SystemsWindow>
    {
        public override void Update()
        {
            SafeDisplay = Display;
            if (Display && (DisplayFast || Time.realtimeSinceStartup - LastUpdateTime > DisplayUpdateSInterval))
            {
                LastUpdateTime = Time.realtimeSinceStartup;
                VesselChangeProfilerText = "Upd: " + VesselChangeSystem.Singleton.UpdateProfiler;
                VesselChangeProfilerText += "Fixed Upd: " + VesselChangeSystem.Singleton.FixedUpdateProfiler;

                VesselDockProfilerText = "Upd: " + VesselDockSystem.Singleton.UpdateProfiler;
                VesselDockProfilerText += "Fixed Upd: " + VesselDockSystem.Singleton.FixedUpdateProfiler;

                VesselFlightStateProfilerText = "Upd: " + VesselFlightStateSystem.Singleton.UpdateProfiler;
                VesselFlightStateProfilerText += "Fixed Upd: " + VesselFlightStateSystem.Singleton.FixedUpdateProfiler;

                VesselImmortalProfilerText = "Upd: " + VesselImmortalSystem.Singleton.UpdateProfiler;
                VesselImmortalProfilerText += "Fixed Upd: " + VesselImmortalSystem.Singleton.FixedUpdateProfiler;

                VesselLockProfilerText = "Upd: " + VesselLockSystem.Singleton.UpdateProfiler;
                VesselLockProfilerText += "Fixed Upd: " + VesselLockSystem.Singleton.FixedUpdateProfiler;

                VesselPositionProfilerText = "Upd: " + VesselPositionSystem.Singleton.UpdateProfiler;
                VesselPositionProfilerText += "Fixed Upd: " + VesselPositionSystem.Singleton.FixedUpdateProfiler;

                VesselProtoProfilerText = "Upd: " + VesselProtoSystem.Singleton.UpdateProfiler;
                VesselProtoProfilerText += "Fixed Upd: " + VesselProtoSystem.Singleton.FixedUpdateProfiler;

                VesselRangeProfilerText = "Upd: " + VesselRangeSystem.Singleton.UpdateProfiler;
                VesselRangeProfilerText += "Fixed Upd: " + VesselRangeSystem.Singleton.FixedUpdateProfiler;

                VesselRemoveProfilerText = "Upd: " + VesselRemoveSystem.Singleton.UpdateProfiler;
                VesselRemoveProfilerText += "Fixed Upd: " + VesselRemoveSystem.Singleton.FixedUpdateProfiler;

                VesselUpdateProfilerText = "Upd: " + VesselUpdateSystem.Singleton.UpdateProfiler;
                VesselUpdateProfilerText += "Fixed Upd: " + VesselUpdateSystem.Singleton.FixedUpdateProfiler;
            }
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

            if (SafeDisplay)
            {
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

        #region Fields

        private float WindowHeight { get; } = 400;
        private float WindowWidth { get; } = 400;

        private bool VesselSystems { get; set; }
        private bool DisplayFast { get; set; }
        private float LastUpdateTime { get; set; }
        private float DisplayUpdateSInterval { get; } = 1f;

        #region Vessel systems

        private bool VesselChange { get; set; }
        private string VesselChangeProfilerText { get; set; }

        private bool VesselDock { get; set; }
        private string VesselDockProfilerText { get; set; }

        private bool VesselFlightState { get; set; }
        private string VesselFlightStateProfilerText { get; set; }

        private bool VesselImmortal { get; set; }
        private string VesselImmortalProfilerText { get; set; }

        private bool VesselLock { get; set; }
        private string VesselLockProfilerText { get; set; }

        private bool VesselPosition { get; set; }
        private string VesselPositionProfilerText { get; set; }

        private bool VesselProto { get; set; }
        private string VesselProtoProfilerText { get; set; }

        private bool VesselRange { get; set; }
        private string VesselRangeProfilerText { get; set; }

        private bool VesselRemove { get; set; }
        private string VesselRemoveProfilerText { get; set; }

        private bool VesselUpdate { get; set; }
        private string VesselUpdateProfilerText { get; set; }

        #endregion

        #endregion
    }
}
