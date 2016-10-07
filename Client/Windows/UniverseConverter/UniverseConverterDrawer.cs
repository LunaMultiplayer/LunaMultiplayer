using UnityEngine;

namespace LunaClient.Windows.UniverseConverter
{
    public partial class UniverseConverterWindow
    {
        public void DrawContent(int windowId)
        {
            GUI.DragWindow(MoveRect);
            GUILayout.BeginVertical();
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