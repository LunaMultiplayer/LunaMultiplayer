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

            _displayPositions = GUILayout.Toggle(_displayPositions, "Display vessel positions", ButtonStyle);
            if (_displayPositions)
                GUILayout.Label(_positionText, LabelStyle);

            _displayFloatingOrigin = GUILayout.Toggle(_displayFloatingOrigin, "Display flotating origin", ButtonStyle);
            if (_displayFloatingOrigin)
                GUILayout.Label(_floatingOriginText, LabelStyle);

            _displayOrbit = GUILayout.Toggle(_displayOrbit, "Display active vessel orbit info", ButtonStyle);
            if (_displayOrbit)
                GUILayout.Label(_orbitText, LabelStyle);

            _displayVesselsOrbit = GUILayout.Toggle(_displayVesselsOrbit, "Display other vessels orbit info", ButtonStyle);
            if (_displayVesselsOrbit)
                GUILayout.Label(_orbitVesselsText, LabelStyle);

            _displayInterpolationData = GUILayout.Toggle(_displayInterpolationData, "Display interpolation statistics", ButtonStyle);
            if (_displayInterpolationData)
            {
                GUILayout.Label(_interpolationText, LabelStyle);
            }

            _displaySubspace = GUILayout.Toggle(_displaySubspace, "Display subspace statistics", ButtonStyle);
            if (_displaySubspace)
                GUILayout.Label(_subspaceText, LabelStyle);

            _displayTimes = GUILayout.Toggle(_displayTimes, "Display time clocks", ButtonStyle);
            if (_displayTimes)
                GUILayout.Label(_timeText, LabelStyle);

            _displayConnectionQueue = GUILayout.Toggle(_displayConnectionQueue, "Display connection statistics", ButtonStyle);
            if (_displayConnectionQueue)
                GUILayout.Label(_connectionText, LabelStyle);

            GUILayout.EndVertical();
        }
    }
}
