using Harmony;
using LmpClient.VesselUtilities;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to disable the warnings when going back to KSC if we are spectating
    /// </summary>
    [HarmonyPatch(typeof(PauseMenu))]
    [HarmonyPatch("Display")]
    public class PauseMenu_Display
    {
        [HarmonyPostfix]
        private static void PostfixDisplay()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            if (VesselCommon.IsSpectating && PauseMenu.exists && PauseMenu.isOpen)
                PauseMenu.canSaveAndExit = ClearToSaveStatus.CLEAR;
        }
    }
}
