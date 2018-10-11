using UnityEngine;

namespace LmpClient.Windows.Debug
{
    public partial class DebugWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            _displayFast = GUILayout.Toggle(_displayFast, "Fast debug update");

            _displayVectors = GUILayout.Toggle(_displayVectors, "Display vessel vectors");
            if (_displayVectors)
                GUILayout.Label(_vectorText);

            _displayPositions = GUILayout.Toggle(_displayPositions, "Display active vessel positions");
            if (_displayPositions)
                GUILayout.Label(_positionText);

            _displayVesselsPositions = GUILayout.Toggle(_displayVesselsPositions, "Display other vessel positions");
            if (_displayVesselsPositions)
                GUILayout.Label(_positionVesselsText);

            _displayOrbit = GUILayout.Toggle(_displayOrbit, "Display active vessel orbit info");
            if (_displayOrbit)
                GUILayout.Label(_orbitText);

            _displayVesselsOrbit = GUILayout.Toggle(_displayVesselsOrbit, "Display other vessels orbit info");
            if (_displayVesselsOrbit)
                GUILayout.Label(_orbitVesselsText);

            _displayInterpolationData = GUILayout.Toggle(_displayInterpolationData, "Display interpolation statistics");
            if (_displayInterpolationData)
            {
                GUILayout.Label(_interpolationText);
            }

            _displaySubspace = GUILayout.Toggle(_displaySubspace, "Display subspace statistics");
            if (_displaySubspace)
                GUILayout.Label(_subspaceText);

            _displayTimes = GUILayout.Toggle(_displayTimes, "Display time clocks");
            if (_displayTimes)
                GUILayout.Label(_timeText);

            _displayConnectionQueue = GUILayout.Toggle(_displayConnectionQueue, "Display connection statistics");
            if (_displayConnectionQueue)
                GUILayout.Label(_connectionText);

            GUILayout.EndVertical();
        }
    }
}
