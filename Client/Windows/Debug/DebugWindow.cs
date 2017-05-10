using System;
using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems;
using LunaClient.Systems.Lock;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using UniLinq;
using UnityEngine;

namespace LunaClient.Windows.Debug
{
    public partial class DebugWindow : Window<DebugWindow>
    {
        public override void Update()
        {
            SafeDisplay = Display;

            if ((Display && (Time.realtimeSinceStartup - LastUpdateTime > DisplayUpdateInterval)) || DisplayFast)
            {
                LastUpdateTime = Time.realtimeSinceStartup;
                //Vector text
                if ((HighLogic.LoadedScene == GameScenes.FLIGHT) && FlightGlobals.ready &&
                    (FlightGlobals.ActiveVessel != null))
                {
                    var ourVessel = FlightGlobals.ActiveVessel;
                    VectorText = "Forward vector: " + ourVessel.GetFwdVector() + "\n";
                    VectorText += "Up vector: " + (Vector3) ourVessel.upAxis + "\n";
                    VectorText += "Srf Rotation: " + ourVessel.srfRelRotation + "\n";
                    VectorText += "Vessel Rotation: " + ourVessel.transform.rotation + "\n";
                    VectorText += "Vessel Local Rotation: " + ourVessel.transform.localRotation + "\n";
                    VectorText += "mainBody Rotation: " + (Quaternion) ourVessel.mainBody.rotation + "\n";
                    VectorText += "mainBody Transform Rotation: " + ourVessel.mainBody.bodyTransform.rotation + "\n";
                    VectorText += "Surface Velocity: " + ourVessel.GetSrfVelocity() + ", |v|: " +
                                  ourVessel.GetSrfVelocity().magnitude + "\n";
                    VectorText += "Orbital Velocity: " + ourVessel.GetObtVelocity() + ", |v|: " +
                                  ourVessel.GetObtVelocity().magnitude + "\n";
                    if ((ourVessel.orbitDriver != null) && (ourVessel.orbitDriver.orbit != null))
                        VectorText += "Frame Velocity: " + (Vector3) ourVessel.orbitDriver.orbit.GetFrameVel() +
                                      ", |v|: " + ourVessel.orbitDriver.orbit.GetFrameVel().magnitude + "\n";
                    VectorText += "CoM offset vector: " + ourVessel.CoM + "\n";
                    VectorText += "Angular Velocity: " + ourVessel.angularVelocity + ", |v|: " +
                                  ourVessel.angularVelocity.magnitude + "\n";
                    VectorText += "World Pos: " + (Vector3) ourVessel.GetWorldPos3D() + ", |pos|: " +
                                  ourVessel.GetWorldPos3D().magnitude + "\n";
                }
                else
                {
                    VectorText = "You have to be in flight";
                }

                //NTP text
                NtpText = $"Warp rate: {Math.Round(Time.timeScale, 3)}x.\n";
                NtpText += $"Current subspace: {WarpSystem.Singleton.CurrentSubspace}.\n";
                NtpText += $"Current Error: {Math.Round(TimeSyncerSystem.Singleton.GetCurrentError()*1000, 0)}ms.\n";
                NtpText += $"Current universe time: {Math.Round(Planetarium.GetUniversalTime(), 3)} UT\n";
                NtpText += $"Network latency: {Math.Round(TimeSyncerSystem.Singleton.NetworkLatencyAverage/10000f, 3)}ms\n";
                NtpText += $"Server clock difference: {Math.Round(TimeSyncerSystem.Singleton.ClockOffsetAverage/10000f, 3)}ms\n";
                NtpText += $"Server lag: {Math.Round(TimeSyncerSystem.Singleton.ServerLag/10000f, 3)}ms\n";

                //Connection queue text
                ConnectionText = $"Ping: {NetworkStatistics.GetStatistics("Ping")}ms.\n";
                ConnectionText += $"Last send time: {NetworkStatistics.GetStatistics("LastSendTime")}ms ago.\n";
                ConnectionText += $"Last receive time: {NetworkStatistics.GetStatistics("LastReceiveTime")}ms ago.\n";
                ConnectionText += $"Sent bytes: {NetworkStatistics.GetStatistics("SentBytes")}.\n";
                ConnectionText += $"Received bytes: {NetworkStatistics.GetStatistics("ReceivedBytes")}.\n";
                ConnectionText += $"Queued out msgs: {NetworkStatistics.GetStatistics("QueuedOutgoingMessages")}.\n";

                //Vessel update system
                VesselUpdateText = $"Queued messages: {VesselUpdateSystem.Singleton.MessageHandler.IncomingMessages.Count}.\n";
                VesselUpdateText += $"Spectating: {VesselCommon.IsSpectating}.\n";
                VesselUpdateText += "Active vessel control lock: " +
                    $"{FlightGlobals.ActiveVessel != null && LockSystem.Singleton.LockIsOurs("control-" + FlightGlobals.ActiveVessel.id)}.\n";
                VesselUpdateText += "Active vessel update lock: " +
                    $"{FlightGlobals.ActiveVessel != null && LockSystem.Singleton.LockIsOurs("update-" + FlightGlobals.ActiveVessel.id)}.\n";

                ProfilerText = "Update: " + LunaProfiler.UpdateData;
                ProfilerText += "Fixed Update: " + LunaProfiler.FixedUpdateData;
                ProfilerText += "GUI: " + LunaProfiler.GuiData;
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
            WindowRect = new Rect(Screen.width - (WindowWidth + 50), Screen.height/2f - WindowHeight/2f, WindowWidth,
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
        public string VesselUpdateText { get; set; } = "";
        public string NtpText { get; set; } = "";
        public string ConnectionText { get; set; } = "";
        public string ProfilerText { get; set; } = "";
        public float LastUpdateTime { get; set; }
        private float DisplayUpdateInterval { get; } = .2f;

        #endregion

        private float WindowHeight { get; } = 400;
        private float WindowWidth { get; } = 400;

        protected bool DisplayVectors { get; set; }
        protected bool DisplayNtp { get; set; }
        protected bool DisplayConnectionQueue { get; set; }
        protected bool DisplayVesselUpdatesData { get; set; }
        protected bool DisplayProfilerStatistics { get; set; }

        #endregion
    }
}