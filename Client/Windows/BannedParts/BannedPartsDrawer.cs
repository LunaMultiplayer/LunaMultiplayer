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
            GUILayout.Label($"{VesselName} ({VesselId}) {LocalizationContainer.BannedPartsWindowText.Text}", BoldLabelStyle);
            GUILayout.Space(5);

            GUILayout.BeginVertical(BoxStyle);
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
            foreach (var bannedPart in BannedParts)
            {
                GUILayout.Label(bannedPart, LabelStyle);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (GUILayout.Button(LocalizationContainer.BannedPartsWindowText.Close, ButtonStyle))
                Display = false;
            GUILayout.EndVertical();
        }
    }
}