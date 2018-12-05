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
            FastUpdate = GUILayout.Toggle(FastUpdate, "Fast Update");
            FilterAsteroids = GUILayout.Toggle(FilterAsteroids, "Filter Asteroids");
            if (ActiveVesselDisplayStore != null)
            { 
                GUILayout.Label("Active vessel:");
                GUILayout.BeginVertical(Skin.box);
                ActiveVesselDisplayStore.Display = GUILayout.Toggle(ActiveVesselDisplayStore.Display, ActiveVesselDisplayStore.VesselId.ToString());
                ActiveVesselDisplayStore.Print();
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
