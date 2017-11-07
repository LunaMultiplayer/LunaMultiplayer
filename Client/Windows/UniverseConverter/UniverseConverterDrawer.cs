using UnityEngine;

namespace LunaClient.Windows.UniverseConverter
{
    public partial class UniverseConverterWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
            foreach (var saveFolder in SaveDirectories)
                if (GUILayout.Button(saveFolder))
                    Utilities.UniverseConverter.GenerateUniverse(saveFolder);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", ButtonStyle))
                Display = false;
            GUILayout.EndVertical();
        }
    }
}