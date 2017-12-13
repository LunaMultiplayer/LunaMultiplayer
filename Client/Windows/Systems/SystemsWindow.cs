using LunaClient.Base;
using LunaClient.Systems;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Facility;
using LunaClient.Systems.Flag;
using LunaClient.Systems.GameScene;
using LunaClient.Systems.Groups;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Mod;
using LunaClient.Systems.ModApi;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Toolbar;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselImmortalSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRangeSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.Warp;
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
            get => _display && MainSystem.NetworkState >= ClientState.Running &&
                   HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => _display = value;
        }

        private float WindowHeight { get; } = 400;
        private float WindowWidth { get; } = 400;

        private bool DisplayFast { get; set; }
        private float LastUpdateTime { get; set; }
        private float DisplayUpdateSInterval { get; } = 1f;

        private string LmpProfilerText { get; set; }

        #region Vessel systems

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

        private bool VesselSwitcher { get; set; }
        private string VesselSwitcherProfilerText { get; set; }

        #endregion

        #region Other systems

        private bool Asteroid { get; set; }
        private string AsteroidProfilerText { get; set; }

        private bool CraftLibrary { get; set; }
        private string CraftLibraryProfilerText { get; set; }

        private bool Facility { get; set; }
        private string FacilityProfilerText { get; set; }

        private bool Flag { get; set; }
        private string FlagProfilerText { get; set; }

        private bool GameScene { get; set; }
        private string GameSceneProfilerText { get; set; }

        private bool Group { get; set; }
        private string GroupProfilerText { get; set; }

        private bool Kerbal { get; set; }
        private string KerbalProfilerText { get; set; }

        private bool Lock { get; set; }
        private string LockProfilerText { get; set; }

        private bool ModS { get; set; }
        private string ModProfilerText { get; set; }

        private bool ModApi { get; set; }
        private string ModApiProfilerText { get; set; }

        private bool PlayerColor { get; set; }
        private string PlayerColorProfilerText { get; set; }

        private bool PlayerConnection { get; set; }
        private string PlayerConnectionProfilerText { get; set; }

        private bool Scenario { get; set; }
        private string ScenarioProfilerText { get; set; }

        private bool TimeSyncer { get; set; }
        private string TimeSyncerProfilerText { get; set; }

        private bool Toolbar { get; set; }
        private string ToolbarProfilerText { get; set; }

        private bool Warp { get; set; }
        private string WarpProfilerText { get; set; }

        #endregion

        #endregion

        public override void Update()
        {
            SafeDisplay = Display;
            if (Display && (DisplayFast || Time.realtimeSinceStartup - LastUpdateTime > DisplayUpdateSInterval))
            {
                LastUpdateTime = Time.realtimeSinceStartup;

                LmpProfilerText = LunaProfiler.GetProfilersData();

                AsteroidProfilerText = SystemsContainer.Get<AsteroidSystem>().GetProfilersData();
                CraftLibraryProfilerText = SystemsContainer.Get<CraftLibrarySystem>().GetProfilersData();
                FacilityProfilerText = SystemsContainer.Get<FacilitySystem>().GetProfilersData();
                FlagProfilerText = SystemsContainer.Get<FlagSystem>().GetProfilersData();
                GameSceneProfilerText = SystemsContainer.Get<GameSceneSystem>().GetProfilersData();
                GroupProfilerText = SystemsContainer.Get<GroupSystem>().GetProfilersData();
                KerbalProfilerText = SystemsContainer.Get<KerbalSystem>().GetProfilersData();
                LockProfilerText = SystemsContainer.Get<LockSystem>().GetProfilersData();
                ModProfilerText = SystemsContainer.Get<ModSystem>().GetProfilersData();
                ModApiProfilerText = SystemsContainer.Get<ModApiSystem>().GetProfilersData();
                PlayerColorProfilerText = SystemsContainer.Get<PlayerColorSystem>().GetProfilersData();
                PlayerConnectionProfilerText = SystemsContainer.Get<PlayerConnectionSystem>().GetProfilersData();
                ScenarioProfilerText = SystemsContainer.Get<ScenarioSystem>().GetProfilersData();
                TimeSyncerProfilerText = SystemsContainer.Get<TimeSyncerSystem>().GetProfilersData();
                ToolbarProfilerText = SystemsContainer.Get<ToolbarSystem>().GetProfilersData();
                VesselFlightStateProfilerText = SystemsContainer.Get<VesselFlightStateSystem>().GetProfilersData();
                VesselImmortalProfilerText = SystemsContainer.Get<VesselImmortalSystem>().GetProfilersData();
                VesselLockProfilerText = SystemsContainer.Get<VesselLockSystem>().GetProfilersData();
                VesselPositionProfilerText = SystemsContainer.Get<VesselPositionSystem>().GetProfilersData();
                VesselProtoProfilerText = SystemsContainer.Get<VesselProtoSystem>().GetProfilersData();
                VesselRangeProfilerText = SystemsContainer.Get<VesselRangeSystem>().GetProfilersData();
                VesselRemoveProfilerText = SystemsContainer.Get<VesselRemoveSystem>().GetProfilersData();
                VesselSwitcherProfilerText = SystemsContainer.Get<VesselSwitcherSystem>().GetProfilersData();
                WarpProfilerText = SystemsContainer.Get<WarpSystem>().GetProfilersData();
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
            if (MainSystem.NetworkState < ClientState.Running || HighLogic.LoadedSceneIsFlight)
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
    }
}
