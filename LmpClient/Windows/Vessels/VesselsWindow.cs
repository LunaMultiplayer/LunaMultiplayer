using LmpClient.Base;
using LmpClient.Extensions;
using LmpCommon.Enums;
using LmpCommon.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LmpClient.Windows.Vessels
{
    public partial class VesselsWindow : Window<VesselsWindow>
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

        private static readonly List<VesselDisplay> Vessels = new List<VesselDisplay>();
        private static readonly StringBuilder StrBuilder = new StringBuilder();

        #endregion

        public override void Update()
        {
            base.Update();
            if (Display && TimeUtil.IsInInterval(ref _lastUpdateTime, 3000))
            {
                for (var i = 0; i < FlightGlobals.Vessels.Count; i++)
                {
                    if (FlightGlobals.Vessels[i] == null) continue;

                    var existingVessel = Vessels.FirstOrDefault(v => v != null && v.VesselId == FlightGlobals.Vessels[i].id);
                    if (existingVessel == null)
                    {
                        Vessels.Add(new VesselDisplay
                        {
                            VesselId = FlightGlobals.Vessels[i].id,
                            Loaded = FlightGlobals.Vessels[i].loaded,
                            Packed = FlightGlobals.Vessels[i].packed,
                            Immortal = FlightGlobals.Vessels[i].IsImmortal(),
                            VesselName = FlightGlobals.Vessels[i].name,
                            ObtDriverMode = FlightGlobals.Vessels[i].orbitDriver ? FlightGlobals.Vessels[i].orbitDriver.updateMode : OrbitDriver.UpdateMode.IDLE
                        });
                    }
                    else
                    {
                        existingVessel.Loaded = FlightGlobals.Vessels[i].loaded;
                        existingVessel.Packed = FlightGlobals.Vessels[i].packed;
                        existingVessel.Immortal = FlightGlobals.Vessels[i].IsImmortal();
                        existingVessel.VesselName = FlightGlobals.Vessels[i].name;
                        existingVessel.ObtDriverMode = FlightGlobals.Vessels[i].orbitDriver ? FlightGlobals.Vessels[i].orbitDriver.updateMode : OrbitDriver.UpdateMode.IDLE;
                    }
                }

                Vessels.RemoveAll(v => FlightGlobals.Vessels.All(ev => ev != null && ev.id != v.VesselId));
            }
        }

        protected override void DrawGui()
        {
            GUI.skin = DefaultSkin;
            WindowRect = FixWindowPos(GUILayout.Window(6725 + MainSystem.WindowOffset, WindowRect, DrawContent, "Vessels", Skin.window, LayoutOptions));
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
                InputLockManager.RemoveControlLock("LMP_VesselsWindowsLock");
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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_VesselsWindowsLock");
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
