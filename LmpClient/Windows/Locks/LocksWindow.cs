using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpCommon.Enums;
using LmpCommon.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LmpClient.Windows.Locks
{
    /// <summary>
    /// Here we should only display the statistics for systems that contain ROUTINES or code that executes on every fixedupdate/update/lateupdate
    /// </summary>
    public partial class LocksWindow : Window<LocksWindow>
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

        private DateTime _lastUpdateTime = DateTime.MinValue;

        private static readonly List<VesselLockDisplay> VesselLocks = new List<VesselLockDisplay>();
        private static string _asteroidLockOwner = string.Empty;
        private static string _contractLockOwner = string.Empty;
        private static readonly StringBuilder StrBuilder = new StringBuilder();

        #endregion

        public override void Update()
        {
            base.Update();
            if (Display && TimeUtil.IsInInterval(ref _lastUpdateTime, 3000))
            {
                _asteroidLockOwner = LockSystem.LockQuery.AsteroidLockOwner();
                _contractLockOwner = LockSystem.LockQuery.ContractLockOwner();

                for (var i = 0; i < FlightGlobals.Vessels.Count; i++)
                {
                    if (FlightGlobals.Vessels[i] == null) continue;

                    var existingVesselLock = VesselLocks.FirstOrDefault(v => v != null && v.VesselId == FlightGlobals.Vessels[i].id);
                    if (existingVesselLock == null)
                    {
                        VesselLocks.Add(new VesselLockDisplay
                        {
                            VesselId = FlightGlobals.Vessels[i].id,
                            Loaded = FlightGlobals.Vessels[i].loaded,
                            Packed = FlightGlobals.Vessels[i].packed,
                            Immortal = float.IsPositiveInfinity(FlightGlobals.Vessels[i].rootPart?.crashTolerance ?? 0),
                            VesselName = FlightGlobals.Vessels[i].name,
                            ControlLockOwner = LockSystem.LockQuery.GetControlLockOwner(FlightGlobals.Vessels[i].id),
                            UpdateLockOwner = LockSystem.LockQuery.GetUpdateLockOwner(FlightGlobals.Vessels[i].id),
                            UnloadedUpdateLockOwner = LockSystem.LockQuery.GetUnloadedUpdateLockOwner(FlightGlobals.Vessels[i].id)
                        });
                    }
                    else
                    {
                        existingVesselLock.Loaded = FlightGlobals.Vessels[i].loaded;
                        existingVesselLock.Packed = FlightGlobals.Vessels[i].packed;
                        existingVesselLock.Immortal = float.IsPositiveInfinity(FlightGlobals.Vessels[i].rootPart?.crashTolerance ?? 0);
                        existingVesselLock.VesselName = FlightGlobals.Vessels[i].name;
                        existingVesselLock.ControlLockOwner = LockSystem.LockQuery.GetControlLockOwner(FlightGlobals.Vessels[i].id);
                        existingVesselLock.UpdateLockOwner = LockSystem.LockQuery.GetUpdateLockOwner(FlightGlobals.Vessels[i].id);
                        existingVesselLock.UnloadedUpdateLockOwner = LockSystem.LockQuery.GetUnloadedUpdateLockOwner(FlightGlobals.Vessels[i].id);
                    }
                }

                VesselLocks.RemoveAll(v => FlightGlobals.Vessels.All(ev => ev != null && ev.id != v.VesselId));
            }
        }

        protected override void DrawGui()
        {
            WindowRect = FixWindowPos(GUILayout.Window(6717 + MainSystem.WindowOffset, WindowRect, DrawContent, "Locks", skin.window, LayoutOptions));
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width - (WindowWidth + 50), Screen.height / 2f - WindowHeight / 2f, WindowWidth,
                WindowHeight);
            MoveRect = new Rect(0, 0, int.MaxValue, TitleHeight);

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
                InputLockManager.RemoveControlLock("LMP_LocksWindowsLock");
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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_LocksWindowsLock");
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
