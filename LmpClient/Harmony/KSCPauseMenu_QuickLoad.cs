using Harmony;
using LmpClient.Localization;
using LmpCommon.Enums;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to deny the loading while in a multiplayer game and in the KSC screen
    /// </summary>
    [HarmonyPatch(typeof(KSCPauseMenu))]
    [HarmonyPatch("quickLoad")]
    public class KSCPauseMenu_QuickLoad
    {
        [HarmonyPrefix]
        private static bool PrefixQuickLoad()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.CannotLoadGames, 5f, ScreenMessageStyle.UPPER_CENTER);

            return false;
        }
    }
}
