using LunaClient.Base;
using LunaClient.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Windows.UniverseConverter
{
    public partial class UniverseConverterWindow : Window<UniverseConverterWindow>
    {
        private const float WindowHeight = 300;
        private const float WindowWidth = 200;

        public bool LoadEventHandled { get; } = true;
        protected IEnumerable<string> SaveDirectories { get; } = Utilities.UniverseConverter.GetSavedNames();
        
        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
                WindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6712 + MainSystem.WindowOffset, WindowRect,
                        DrawContent, "Universe Converter", WindowStyle, LayoutOptions));
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width/4f - WindowWidth/2f, Screen.height/2f - WindowHeight/2f, WindowWidth,
                WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.Width(WindowWidth);
            LayoutOptions[1] = GUILayout.Height(WindowHeight);
            LayoutOptions[2] = GUILayout.ExpandWidth(true);
            LayoutOptions[3] = GUILayout.ExpandHeight(true);
        }

        public override void Update()
        {
            base.Update();
            SafeDisplay = Display;
        }
    }
}