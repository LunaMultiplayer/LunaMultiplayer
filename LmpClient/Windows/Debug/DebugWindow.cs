using LmpClient.Base;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.Warp;
using LmpCommon.Enums;
using LmpCommon.Time;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LmpClient.Windows.Debug
{
    public partial class DebugWindow : Window<DebugWindow>
    {
        #region Fields

        private static readonly StringBuilder StringBuilder = new StringBuilder();
        private static readonly List<Tuple<Guid, string>> VesselProtoStoreData = new List<Tuple<Guid, string>>();

        private const float DisplayUpdateInterval = .2f;
        private const float WindowHeight = 400;
        private const float WindowWidth = 650;

        private static bool _displayFast;
        private static string _subspaceText;
        private static string _timeText;
        private static string _connectionText;
        private static float _lastUpdateTime;

        private static bool _displaySubspace;
        private static bool _displayTimes;
        private static bool _displayConnectionQueue;

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => base.Display = _display = value;
        }

        #endregion

        public override void Update()
        {
            base.Update();
            if (Display && Time.realtimeSinceStartup - _lastUpdateTime > DisplayUpdateInterval || _displayFast)
            {
                _lastUpdateTime = Time.realtimeSinceStartup;

                if (_displaySubspace)
                {
                    StringBuilder.AppendLine($"Warp rate: {Math.Round(Time.timeScale, 3)}x.");
                    StringBuilder.AppendLine($"Current subspace: {WarpSystem.Singleton.CurrentSubspace}.");
                    StringBuilder.AppendLine($"Current subspace time: {WarpSystem.Singleton.CurrentSubspaceTime}s.");
                    StringBuilder.AppendLine($"Current subspace time difference: {WarpSystem.Singleton.CurrentSubspaceTimeDifference}s.");
                    StringBuilder.AppendLine($"Current Error: {Math.Round(TimeSyncSystem.CurrentErrorSec * 1000, 0)}ms.");
                    StringBuilder.AppendLine($"Current universe time: {Math.Round(TimeSyncSystem.UniversalTime, 3)} UT");

                    _subspaceText = StringBuilder.ToString();
                    StringBuilder.Length = 0;
                }

                if (_displayTimes)
                {
                    StringBuilder.AppendLine($"Server start time: {new DateTime(TimeSyncSystem.ServerStartTime):yyyy-MM-dd HH-mm-ss.ffff}");

                    StringBuilder.AppendLine($"Computer clock time (UTC): {DateTime.UtcNow:HH:mm:ss.fff}");
                    StringBuilder.AppendLine($"Computer clock offset (minutes): {LunaComputerTime.SimulatedMinutesTimeOffset}");
                    StringBuilder.AppendLine($"Computer clock time + offset: {LunaComputerTime.UtcNow:HH:mm:ss.fff}");
                    
                    StringBuilder.AppendLine($"Computer <-> NTP clock difference: {LunaNetworkTime.TimeDifference.TotalMilliseconds}ms.");
                    StringBuilder.AppendLine($"NTP clock offset: {LunaNetworkTime.SimulatedMsTimeOffset}ms.");
                    StringBuilder.AppendLine($"Total Difference: {LunaNetworkTime.TimeDifference.TotalMilliseconds + LunaNetworkTime.SimulatedMsTimeOffset}ms.");

                    StringBuilder.AppendLine($"NTP clock time (UTP): {LunaNetworkTime.UtcNow:HH:mm:ss.fff}");

                    _timeText = StringBuilder.ToString();
                    StringBuilder.Length = 0;
                }

                if (_displayConnectionQueue)
                {
                    StringBuilder.AppendLine($"Ping: {TimeSpan.FromSeconds(NetworkStatistics.PingSec).TotalMilliseconds}ms.");
                    StringBuilder.AppendLine($"TimeOffset: {TimeSpan.FromSeconds(NetworkStatistics.TimeOffset).TotalMilliseconds}ms.");
                    StringBuilder.AppendLine($"Messages in cache: {NetworkStatistics.MessagesInCache}.");
                    StringBuilder.AppendLine($"Message data in cache: {NetworkStatistics.MessageDataInCache}.");
                    StringBuilder.AppendLine($"Sent bytes: {NetworkStatistics.SentBytes}.");
                    StringBuilder.AppendLine($"Received bytes: {NetworkStatistics.ReceivedBytes}.\n");
                    _connectionText = StringBuilder.ToString();
                    StringBuilder.Length = 0;
                }
            }
        }

        protected override void DrawGui()
        {
            GUI.skin = DefaultSkin;
            WindowRect = FixWindowPos(GUILayout.Window(6705 + MainSystem.WindowOffset, WindowRect, DrawContent, "Debug", LayoutOptions));
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
                InputLockManager.RemoveControlLock("LMP_DebugLock");
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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_DebugLock");
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
