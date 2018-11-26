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
