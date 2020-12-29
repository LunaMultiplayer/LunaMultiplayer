using HarmonyLib;
using KSP.UI.Screens.DebugToolbar;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
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
