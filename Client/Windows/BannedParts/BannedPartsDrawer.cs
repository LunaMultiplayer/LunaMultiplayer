using LunaClient.Localization;
using UnityEngine;

namespace LunaClient.Windows.BannedParts
{
    public partial class BannedPartsWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GUILayout.Label($"{VesselName} ({VesselId}) {LocalizationContainer.BannedPartsWindowText.Text}", LabelStyle);
            GUILayout.Space(5);
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
            GUILayout.Label(BannedParts, LabelStyle);
            GUILayout.EndScrollView();
            if (GUILayout.Button(LocalizationContainer.BannedPartsWindowText.Close, ButtonStyle))
                Display = false;
            GUILayout.EndVertical();
        }
    }
}