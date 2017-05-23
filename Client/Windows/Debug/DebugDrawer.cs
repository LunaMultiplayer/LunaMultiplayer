using System.Diagnostics;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Windows.Debug
{
    public partial class DebugWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GameEventsBase.debugEvents = GUILayout.Toggle(GameEventsBase.debugEvents, "Debug GameEvents", ButtonStyle);
            DisplayFast = GUILayout.Toggle(DisplayFast, "Fast debug update", ButtonStyle);
            DisplayVectors = GUILayout.Toggle(DisplayVectors, "Display vessel vectors", ButtonStyle);
            if (DisplayVectors)
                GUILayout.Label(VectorText, LabelStyle);
            DisplayNtp = GUILayout.Toggle(DisplayNtp, "Display NTP/Subspace statistics", ButtonStyle);
            if (DisplayNtp)
                GUILayout.Label(NtpText, LabelStyle);
            DisplayConnectionQueue = GUILayout.Toggle(DisplayConnectionQueue, "Display connection statistics", ButtonStyle);
            if (DisplayConnectionQueue)
                GUILayout.Label(ConnectionText, LabelStyle);
            DisplayVesselUpdatesData = GUILayout.Toggle(DisplayVesselUpdatesData, "Display vessel updates statistics", ButtonStyle);
            if (DisplayVesselUpdatesData)
                GUILayout.Label(VesselUpdateText, LabelStyle);
            GUILayout.EndVertical();
        }
    }
}