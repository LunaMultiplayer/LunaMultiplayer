using Harmony;
using KSP.UI.Screens.DebugToolbar;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to hide the windows cheat screen when cheats are disabled
    /// </summary>
    [HarmonyPatch(typeof(DebugScreen))]
    [HarmonyPatch("onGameSceneLoad")]
    public class DebugScreen_OnGameSceneLoad
    {
        [HarmonyPostfix]
        private static void PostFixOnGameSceneLoad(DebugToolbar __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            if (!SettingsSystem.ServerSettings.AllowCheats)
            {
                Traverse.Create(__instance).Field("_cheatsLocked").SetValue(true);
            }
            else
            {
                Traverse.Create(__instance).Field("_cheatsLocked").SetValue(false);
            }
        }
    }
}
