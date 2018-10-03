using System;
using System.IO;
using LmpClient.Base;
using LmpClient.Extensions;
using LmpClient.Utilities;
using LmpCommon.Enums;
using LmpCommon.Time;
using UnityEngine;

namespace LmpClient.Windows.Tools
{
    public partial class ToolsWindow : Window<ToolsWindow>
    {
        #region Fields

        private const float WindowHeight = 400;
        private const float WindowWidth = 400;

        private static bool _saveCurrentOrbitData;

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => base.Display = _display = value;
        }

        private static readonly string OrbitFilePath = CommonUtil.CombinePaths(MainSystem.KspPath, "OrbitData.txt");

        private static readonly int SaveOrbitDataMsInterval = 100;
        private static DateTime _lastSavedOrbitDataTime = DateTime.MinValue;

        private static bool _displayReloads;
        private static bool _displayRanges;
        private static bool _displayFloatingOrigin;

        #endregion

        public override void OnGui()
        {
            base.OnGui();
            if (Display)
            {
                WindowRect = FixWindowPos(GUILayout.Window(6722 + MainSystem.WindowOffset, WindowRect, DrawContent, "Tools", WindowStyle, LayoutOptions));
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
                InputLockManager.RemoveControlLock("LMP_ToolsLock");
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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_ToolsLock");
                    IsWindowLocked = true;
                }
                if (!shouldLock && IsWindowLocked)
                    RemoveWindowLock();
            }

            if (!Display && IsWindowLocked)
                RemoveWindowLock();
        }

        public override void Update()
        {
            base.Update();
            if (_saveCurrentOrbitData && FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.orbitDriver != null)
            {
                if (TimeUtil.IsInInterval(ref _lastSavedOrbitDataTime, SaveOrbitDataMsInterval))
                {
                    File.AppendAllText(OrbitFilePath, $"{LunaComputerTime.Now:HH:mm:ss.ff};{FlightGlobals.ActiveVessel.orbit.PrintOrbitData()}");
                    LunaScreenMsg.PostScreenMessage($"Saving Orbit info to file: {OrbitFilePath}", 1f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
        }

        private static void CreateNewOrbitDataFile()
        {
            File.WriteAllText(OrbitFilePath, "timestamp;inclination;eccentricity;semiMajorAxis;lan;argPeriapsis;meanAnomalyEpoch;epoch;");
        }
    }
}
