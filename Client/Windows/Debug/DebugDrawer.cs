using UnityEngine;

namespace LunaClient.Windows.Debug
{
    public partial class DebugWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            _displayFast = GUILayout.Toggle(_displayFast, "Fast debug update", ButtonStyle);

            _displayVectors = GUILayout.Toggle(_displayVectors, "Display vessel vectors", ButtonStyle);
            if (_displayVectors)
                GUILayout.Label(_vectorText, LabelStyle);

            _displayOrbit = GUILayout.Toggle(_displayOrbit, "Display orbit info", ButtonStyle);
            if (_displayOrbit)
                GUILayout.Label(_orbitText, LabelStyle);

            _displayVesselStoreData = GUILayout.Toggle(_displayVesselStoreData, "Display vessel store data", ButtonStyle);
            if (_displayVesselStoreData)
                GUILayout.Label(_vesselStoreText, LabelStyle);

            _displayInterpolationData = GUILayout.Toggle(_displayInterpolationData, "Display interpolation statistics", ButtonStyle);
            if (_displayInterpolationData)
                GUILayout.Label(_interpolationText, LabelStyle);

            _displayNtp = GUILayout.Toggle(_displayNtp, "Display NTP/Subspace statistics", ButtonStyle);
            if (_displayNtp)
                GUILayout.Label(_ntpText, LabelStyle);

            _displayConnectionQueue = GUILayout.Toggle(_displayConnectionQueue, "Display connection statistics", ButtonStyle);
            if (_displayConnectionQueue)
                GUILayout.Label(_connectionText, LabelStyle);

            GUILayout.EndVertical();
        }
    }
}
