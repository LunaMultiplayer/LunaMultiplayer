using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Utilities;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Windows.Options
{
    public partial class OptionsWindow : Window<OptionsWindow>
    {
        private readonly GUILayoutOption[] _smallOption = { GUILayout.Width(100), GUILayout.ExpandWidth(false) };

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
                WindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6711 + MainSystem.WindowOffset, WindowRect,
                        DrawContent, LocalizationContainer.OptionsWindowText.Title, WindowStyle, LayoutOptions));
            CheckWindowLock();
        }

        public override void Update()
        {
            base.Update();
            SafeDisplay = Display;
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width / 2f - WindowWidth / 2f, Screen.height / 2f - WindowHeight / 2f, WindowWidth,
                WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.Width(WindowWidth);
            LayoutOptions[1] = GUILayout.Height(WindowHeight);
            LayoutOptions[2] = GUILayout.ExpandWidth(true);
            LayoutOptions[3] = GUILayout.ExpandHeight(true);
            
            TempColor = new Color();
            TempColorLabelStyle = new GUIStyle(GUI.skin.label);
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_OptionsLock");
            }
        }

        private void CheckWindowLock()
        {
            if (SafeDisplay)
            {
                if (MainSystem.NetworkState < ClientState.Running || HighLogic.LoadedSceneIsFlight)
                {
                    RemoveWindowLock();
                    return;
                }

                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                var shouldLock = WindowRect.Contains(mousePos);

                if (shouldLock && !IsWindowLocked)
                {
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_OptionsLock");
                    IsWindowLocked = true;
                }
                if (!shouldLock && IsWindowLocked)
                    RemoveWindowLock();
            }

            if (!SafeDisplay && IsWindowLocked)
                RemoveWindowLock();
        }

        #region Fields

        #region Public

        public bool LoadEventHandled { get; set; }

        #endregion

        private const float WindowHeight = 400;
        private const float WindowWidth = 300;
        protected Color TempColor = new Color(1f, 1f, 1f, 1f);
        
        protected GUIStyle TempColorLabelStyle { get; set; }
        protected bool ShowBadNetworkSimulationFields { get; set; }
        protected bool ShowAdvancedNetworkFields { get; set; }
        protected bool InfiniteTimeout { get; set; }

        #endregion
    }
}