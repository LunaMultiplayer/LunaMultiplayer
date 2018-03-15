using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Windows.Mod
{
    public partial class ModWindow : Window<ModWindow>
    {
        private const float WindowHeight = 400;
        private const float WindowWidth = 600;

        private static Vector2 _missingExpansionsScrollPos;
        private static Vector2 _mandatoryFilesNotFoundScrollPos;
        private static Vector2 _mandatoryFilesDifferentShaScrollPos;
        private static Vector2 _forbiddenFilesScrollPos;
        private static Vector2 _nonListedFilesScrollPos;
        private static Vector2 _mandatoryPartsScrollPos;
        private static Vector2 _forbiddenPartsScrollPos;

        public override void Update()
        {
            SafeDisplay = Display;
        }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
                WindowRect =
                    LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6706 + MainSystem.WindowOffset, WindowRect,
                        DrawContent, LocalizationContainer.ModWindowText.Title, WindowStyle, LayoutOptions));
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width/2f - WindowWidth/2f, Screen.height/2f - WindowHeight/2f, WindowWidth,
                WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);
        }
    }
}