using LmpClient.Base;
using LmpClient.Windows.Vessels.Structures;
using LmpCommon.Enums;
using LmpCommon.Time;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private const float WindowHeight = 500;
        private const float WindowWidth = 600;

        private DateTime _lastUpdateTime = DateTime.MinValue;

        private static readonly Dictionary<Guid, VesselDisplay> VesselDisplayStore = new Dictionary<Guid, VesselDisplay>();

        #endregion

        public override void Update()
        {
            base.Update();
            if (Display && TimeUtil.IsInInterval(ref _lastUpdateTime, 3000))
            {
                for (var i = 0; i < FlightGlobals.Vessels.Count; i++)
                {
                    var vessel = FlightGlobals.Vessels[i];
                    if (!VesselDisplayStore.ContainsKey(vessel.id))
                    {
                        VesselDisplayStore.Add(vessel.id, new VesselDisplay(vessel.id));
                    }

                    VesselDisplayStore[vessel.id].Update(vessel);
                }

                var keysToRemove = VesselDisplayStore.Keys.Except(FlightGlobals.Vessels.Select(v => v.id)).ToList();
                foreach (var key in keysToRemove)
                    VesselDisplayStore.Remove(key);
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
