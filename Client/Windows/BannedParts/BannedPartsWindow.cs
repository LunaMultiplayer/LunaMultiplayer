using LunaClient.Base;
using LunaClient.Localization;
using LunaCommon.Enums;
using System;
using UnityEngine;

namespace LunaClient.Windows.BannedParts
{
    public partial class BannedPartsWindow : Window<BannedPartsWindow>
    {
        private const float WindowHeight = 300;
        private const float WindowWidth = 400;

        private static string[] BannedParts { get; set; } = new string[0];
        private static string VesselName { get; set; }
        private static Guid VesselId { get; set; }

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => base.Display = _display = value;
        }

        public override void OnGui()
        {
            base.OnGui();
            if (Display)
            {
                WindowRect = FixWindowPos(GUILayout.Window(6718 + MainSystem.WindowOffset, WindowRect, DrawContent, 
                    LocalizationContainer.BannedPartsWindowText.Title, WindowStyle, LayoutOptions));
            }
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width/2f - WindowWidth/2f, Screen.height/2f - WindowHeight/2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            ScrollPos = new Vector2();
        }

        public void DisplayBannedPartsDialog(Vessel vessel, string[] bannedParts)
        {
            if (!Display)
            {
                VesselName = vessel.vesselName;
                VesselId = vessel.id;
                BannedParts = bannedParts;
                Display = true;
            }
        }
    }
}
