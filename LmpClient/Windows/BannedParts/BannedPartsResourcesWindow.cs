using LmpClient.Base;
using LmpClient.Localization;
using LmpCommon.Enums;
using UnityEngine;

namespace LmpClient.Windows.BannedParts
{
    public partial class BannedPartsResourcesWindow : Window<BannedPartsResourcesWindow>
    {
        #region Fields

        private const float WindowHeight = 300;
        private const float WindowWidth = 400;

        private static string[] _bannedParts = new string[0];
        private static string[] _bannedResources = new string[0];
        private static int _partCount = 0;
        private static string _vesselName;

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => base.Display = _display = value;
        }

        #endregion

        protected override void DrawGui()
        {
            WindowRect = FixWindowPos(GUILayout.Window(6718 + MainSystem.WindowOffset, WindowRect, DrawContent,
                LocalizationContainer.BannedPartsResourcesWindowText.Title, LayoutOptions));
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width / 2f - WindowWidth / 2f, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, int.MaxValue, TitleHeight);

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            ScrollPos = new Vector2();
        }

        public void DisplayBannedPartsResourcesDialog(string vesselName, string[] bannedParts, string[] bannedResources, int partCount = 0)
        {
            if (!Display)
            {
                _vesselName = vesselName;
                _bannedParts = bannedParts;
                _bannedResources = bannedResources;
                _partCount = partCount;
                Display = true;
            }
        }
    }
}
