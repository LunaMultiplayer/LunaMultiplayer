using UnityEngine;

namespace LmpClient.Windows.Vessels
{
    public partial class VesselsWindow
    {
        protected override void DrawWindowContent(int windowId)
        {
            GUI.DragWindow(MoveRect);

            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(WindowWidth), GUILayout.Height(WindowHeight));
            PrintVessels();
            GUILayout.EndScrollView();
        }

        private static void PrintVessels()
        {
            _fastUpdate = GUILayout.Toggle(_fastUpdate, "Fast Update");
            VesselFilter.DrawFilters();
            if (_activeVesselDisplayStore != null)
            {
                GUILayout.Label("Active vessel:");
                GUILayout.BeginVertical(Skin.box);
                _activeVesselDisplayStore.Display = GUILayout.Toggle(_activeVesselDisplayStore.Display, _activeVesselDisplayStore.VesselId.ToString());
                _activeVesselDisplayStore.Print();
                GUILayout.EndVertical();
                GUILayout.Label("Other vessels:");
            }
            else
            {
                GUILayout.Label("Vessels:");
            }

            foreach (var keyVal in VesselDisplayStore)
            {
                GUILayout.BeginVertical(Skin.box);
                keyVal.Value.Display = GUILayout.Toggle(keyVal.Value.Display, keyVal.Value.VesselId.ToString());
                keyVal.Value.Print();
                GUILayout.EndVertical();
            }
        }
    }
}
