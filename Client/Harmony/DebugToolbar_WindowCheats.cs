using Harmony;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
using UnityEngine;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to hide the windows cheat screen when cheats are disabled
    /// </summary>
    [HarmonyPatch(typeof(DebugToolbar))]
    [HarmonyPatch("WindowCheats")]
    public class DebugToolbar_WindowCheats
    {
        [HarmonyPrefix]
        private static bool PrefixWindowCheats(DebugToolbar __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (!SettingsSystem.ServerSettings.AllowCheats)
            {
                GUILayout.Label("Cheats are disabled on this server");
                return false;
            }

            return true;
        }
    }
}
