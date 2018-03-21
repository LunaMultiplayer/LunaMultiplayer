using LunaClient.Base;
using LunaClient.Localization;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Windows.Options
{
    public partial class OptionsWindow : Window<OptionsWindow>
    {
        #region Fields

        #region Public

        public bool LoadEventHandled { get; set; }

        #endregion

        private const float WindowHeight = 400;
        private const float WindowWidth = 300;
        private const float UniverseConverterWindowHeight = 300;
        private const float UniverseConverterWindowWidth = 200;

        protected Color TempColor = new Color(1f, 1f, 1f, 1f);

        protected GUIStyle TempColorLabelStyle { get; set; }
        protected bool ShowBadNetworkSimulationFields { get; set; }
        protected bool ShowAdvancedNetworkFields { get; set; }
        protected bool InfiniteTimeout { get; set; }

        protected Rect UniverseConverterWindowRect { get; set; }
        protected GUILayoutOption[] UniverseConverterLayoutOptions { get; set; }

        private bool DisplayUniverseConverterDialog { get; set; }

        #endregion

        private readonly GUILayoutOption[] _smallOption = { GUILayout.Width(100), GUILayout.ExpandWidth(false) };

        public override void OnGui()
        {
            base.OnGui();
            if (Display)
            {
                WindowRect = FixWindowPos(GUILayout.Window(6711 + MainSystem.WindowOffset, WindowRect, DrawContent, 
                    LocalizationContainer.OptionsWindowText.Title, WindowStyle, LayoutOptions));

                if (DisplayUniverseConverterDialog)
                {
                    UniverseConverterWindowRect = FixWindowPos(GUILayout.Window(6712 + MainSystem.WindowOffset,
                        UniverseConverterWindowRect, DrawUniverseConverterDialog, "Universe converter", WindowStyle, UniverseConverterLayoutOptions));
                }
            }

            CheckWindowLock();
        }
        
        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width / 2f - WindowWidth / 2f, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            UniverseConverterWindowRect = new Rect(Screen.width * 0.025f, Screen.height * 0.025f, WindowWidth, WindowHeight);

            MoveRect = new Rect(0, 0, 10000, 20);
            
            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.Width(WindowWidth);
            LayoutOptions[1] = GUILayout.Height(WindowHeight);
            LayoutOptions[2] = GUILayout.ExpandWidth(true);
            LayoutOptions[3] = GUILayout.ExpandHeight(true);

            UniverseConverterLayoutOptions = new GUILayoutOption[4];
            UniverseConverterLayoutOptions[0] = GUILayout.Width(UniverseConverterWindowWidth);
            UniverseConverterLayoutOptions[1] = GUILayout.Height(UniverseConverterWindowHeight);
            UniverseConverterLayoutOptions[2] = GUILayout.ExpandWidth(true);
            UniverseConverterLayoutOptions[3] = GUILayout.ExpandHeight(true);

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
            if (Display)
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

            if (!Display && IsWindowLocked)
                RemoveWindowLock();
        }
    }
}
