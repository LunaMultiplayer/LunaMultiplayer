using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;

namespace LunaClient.Systems.Toolbar
{
    public class ToolbarSystem : Base.System<ToolbarSystem>
    {
        #region Constructor

        /// <summary>
        /// This system must be ALWAYS enabled so we set it as enabled on the constructor
        /// </summary>
        public ToolbarSystem()
        {
            base.Enabled = true;
            GameEvents.onGUIApplicationLauncherReady.Add(ToolbarEvents.EnableToolBar);
        }

        #endregion

        #region Fields

        public ToolbarEvents ToolbarEvents { get; } = new ToolbarEvents();
        
        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(ToolbarSystem);

        #endregion

        #region Public methods

        public void HandleButtonClick()
        {
            if (!SettingsSystem.CurrentSettings.DisclaimerAccepted)
            {
                DisclaimerDialog.SpawnDialog();
            }
            else
            {
                MainSystem.ToolbarShowGui = !MainSystem.ToolbarShowGui;
            }
        }

        #endregion
    }
}