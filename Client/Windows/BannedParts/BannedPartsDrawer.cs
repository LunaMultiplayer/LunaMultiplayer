using LunaClient.Localization;
using UnityEngine;

namespace LunaClient.Windows.BannedParts
{
    public partial class BannedPartsWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GUILayout.Label($"{_vesselName} ({_vesselId}) {LocalizationContainer.BannedPartsWindowText.Text}", BoldRedLabelStyle);
            GUILayout.Space(5);

            GUILayout.BeginVertical(BoxStyle);
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
            foreach (var bannedPart in _bannedParts)
            {
                GUILayout.Label(bannedPart, LabelStyle);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }
    }
}
