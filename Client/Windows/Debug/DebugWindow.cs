using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Time;
using System;
using System.Text;
using UnityEngine;

namespace LunaClient.Windows.Debug
{
    public partial class DebugWindow : Window<DebugWindow>
    {
        private static readonly StringBuilder StringBuilder = new StringBuilder();

        private static bool _display;
        public override bool Display
        {
            get => _display && MainSystem.NetworkState >= ClientState.Running &&
                   HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => _display = value;
        }

        public override void Update()
        {
            SafeDisplay = Display;

            if (Display && Time.realtimeSinceStartup - LastUpdateTime > DisplayUpdateInterval || DisplayFast)
            {
                LastUpdateTime = Time.realtimeSinceStartup;
                //Vector text

                if (DisplayVectors)
                {
                    if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ready && FlightGlobals.ActiveVessel != null)
                    {
                        var ourVessel = FlightGlobals.ActiveVessel;

                        StringBuilder.AppendLine($"Id: {ourVessel.id}");
                        StringBuilder.AppendLine($"Forward vector: {ourVessel.GetFwdVector()}");
                        StringBuilder.AppendLine($"Up vector: {(Vector3)ourVessel.upAxis}");
                        StringBuilder.AppendLine($"Srf Rotation: {ourVessel.srfRelRotation}");
                        StringBuilder.AppendLine($"Vessel Rotation: {ourVessel.transform.rotation}");
                        StringBuilder.AppendLine($"Vessel Local Rotation: {ourVessel.transform.localRotation}");
                        StringBuilder.AppendLine($"mainBody Rotation: {(Quaternion)ourVessel.mainBody.rotation}");
                        StringBuilder.AppendLine($"mainBody Transform Rotation: {ourVessel.mainBody.bodyTransform.rotation}");
                        StringBuilder.AppendLine($"Surface Velocity: {ourVessel.GetSrfVelocity()}, |v|: {ourVessel.GetSrfVelocity().magnitude}");
                        StringBuilder.AppendLine($"Orbital Velocity: {ourVessel.GetObtVelocity()}, |v|: {ourVessel.GetObtVelocity().magnitude}");
                        if (ourVessel.orbitDriver != null && ourVessel.orbitDriver.orbit != null)
                            StringBuilder.AppendLine($"Frame Velocity: {(Vector3)ourVessel.orbitDriver.orbit.GetFrameVel()}, |v|: {ourVessel.orbitDriver.orbit.GetFrameVel().magnitude}");
                        StringBuilder.AppendLine($"CoM offset vector: {ourVessel.CoM}\n");
                        StringBuilder.AppendLine($"Angular Velocity: {ourVessel.angularVelocity}, |v|: {ourVessel.angularVelocity.magnitude}");
                        StringBuilder.AppendLine($"World Pos: {(Vector3)ourVessel.GetWorldPos3D()}, |pos|: {ourVessel.GetWorldPos3D().magnitude}");

                        VectorText = StringBuilder.ToString();
                        StringBuilder.Length = 0;
                    }
                    else
                    {
                        VectorText = "You have to be in flight";
                    }
                }

                if (DisplayNtp)
                {
                    StringBuilder.AppendLine($"Server start time: {new DateTime(TimeSyncerSystem.ServerStartTime):yyyy-MM-dd HH-mm-ss.ffff}");
                    StringBuilder.AppendLine($"Warp rate: {Math.Round(Time.timeScale, 3)}x.");
                    StringBuilder.AppendLine($"Current subspace: {SystemsContainer.Get<WarpSystem>().CurrentSubspace}.");
                    StringBuilder.AppendLine($"Current subspace time: {SystemsContainer.Get<WarpSystem>().CurrentSubspaceTime}s.");
                    StringBuilder.AppendLine($"Current subspace time difference: {SystemsContainer.Get<WarpSystem>().CurrentSubspaceTimeDifference}s.");
                    StringBuilder.AppendLine($"Current Error: {Math.Round(TimeSyncerSystem.CurrentErrorSec * 1000, 0)}ms.");
                    StringBuilder.AppendLine($"Current universe time: {Math.Round(Planetarium.GetUniversalTime(), 3)} UT");

                    NtpText = StringBuilder.ToString();
                    StringBuilder.Length = 0;
                }

                if (DisplayConnectionQueue)
                {
                    StringBuilder.AppendLine($"NIST time diference: {LunaTime.TimeDifference.TotalMilliseconds}ms.");
                    StringBuilder.AppendLine($"Ping: {NetworkStatistics.GetStatistics("Ping")}ms.");
                    StringBuilder.AppendLine($"Latency: {NetworkStatistics.GetStatistics("Latency")}s.");
                    StringBuilder.AppendLine($"TimeOffset: {TimeSpan.FromTicks(NetworkStatistics.GetStatistics("TimeOffset")).TotalMilliseconds}ms.");
                    StringBuilder.AppendLine($"Last send time: {NetworkStatistics.GetStatistics("LastSendTime")}ms ago.");
                    StringBuilder.AppendLine($"Last receive time: {NetworkStatistics.GetStatistics("LastReceiveTime")}ms ago.");
                    StringBuilder.AppendLine($"Messages in cache: {NetworkStatistics.GetStatistics("MessagesInCache")}.");
                    StringBuilder.AppendLine($"Message data in cache: {NetworkStatistics.GetStatistics("MessageDataInCache")}.");
#if DEBUG
                    StringBuilder.AppendLine($"Sent bytes: {NetworkStatistics.GetStatistics("SentBytes")}.");
                    StringBuilder.AppendLine($"Received bytes: {NetworkStatistics.GetStatistics("ReceivedBytes")}.\n");
#endif
                    ConnectionText = StringBuilder.ToString();
                    StringBuilder.Length = 0;
                }
            }
        }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
                WindowRect =
                    LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6705 + MainSystem.WindowOffset, WindowRect,
                        DrawContent, "LunaMultiPlayer - Debug", WindowStyle, LayoutOptions));
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
                InputLockManager.RemoveControlLock("LMP_DebugLock");
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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_DebugLock");
                    IsWindowLocked = true;
                }
                if (!shouldLock && IsWindowLocked)
                    RemoveWindowLock();
            }

            if (!SafeDisplay && IsWindowLocked)
                RemoveWindowLock();
        }

        #region Fields

        #region Public

        //private parts
        public bool DisplayFast { get; set; }
        public string VectorText { get; set; } = "";
        public string NtpText { get; set; } = "";
        public string ConnectionText { get; set; } = "";
        public float LastUpdateTime { get; set; }
        private float DisplayUpdateInterval { get; } = .2f;

        #endregion

        private float WindowHeight { get; } = 400;
        private float WindowWidth { get; } = 400;

        protected bool DisplayVectors { get; set; }
        protected bool DisplayNtp { get; set; }
        protected bool DisplayConnectionQueue { get; set; }

        #endregion
    }
}