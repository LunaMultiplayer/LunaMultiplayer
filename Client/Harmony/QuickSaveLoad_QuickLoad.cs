using Harmony;
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

            ScreenMessages.PostScreenMessage("Cannot quickload in LMP!", 5f, ScreenMessageStyle.UPPER_CENTER);

            return false;
        }
    }
}
