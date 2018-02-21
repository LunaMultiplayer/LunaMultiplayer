using LunaClient.Localization;
using LunaClient.Systems;
using LunaClient.Systems.Mod;
using UnityEngine;

namespace LunaClient.Windows.Mod
{
    public partial class ModWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GUILayout.Label(LocalizationContainer.ModWindowText.Failed, LabelStyle);
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
            GUILayout.Label(SystemsContainer.Get<ModSystem>().FailText, LabelStyle);
            GUILayout.EndScrollView();
            if (GUILayout.Button(LocalizationContainer.ModWindowText.Close, ButtonStyle))
                Display = false;
            GUILayout.EndVertical();
        }
    }
}