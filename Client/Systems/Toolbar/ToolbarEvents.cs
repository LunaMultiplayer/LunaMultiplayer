using KSP.UI.Screens;
using LunaClient.Base;

namespace LunaClient.Systems.Toolbar
{
    public class ToolbarEvents: SubSystem<ToolbarSystem>
    {
        public void EnableToolBar()
        {
            var buttonTexture = GameDatabase.Instance.GetTexture("LunaMultiplayer/Button/LMPButton", false);
            GameEvents.onGUIApplicationLauncherReady.Remove(EnableToolBar);
            ApplicationLauncher.Instance.AddModApplication(System.HandleButtonClick, System.HandleButtonClick,
                () => { }, () => { }, () => { }, () => { }, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
        }
    }
}
