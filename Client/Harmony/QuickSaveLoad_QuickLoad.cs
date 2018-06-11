using Harmony;
using LunaClient.Localization;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to deny the loading while in a multiplayer game
    /// </summary>
    [HarmonyPatch(typeof(QuickSaveLoad))]
    [HarmonyPatch("quickLoad")]
    public class QuickSaveLoad_QuickLoad
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
