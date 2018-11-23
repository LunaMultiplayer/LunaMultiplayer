using Harmony;
using LmpClient.Localization;
using LmpClient.Systems.Revert;
using LmpClient.VesselUtilities;
using LmpCommon.Enums;
using System.Collections.Generic;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to disable the revert after switching vessels
    /// </summary>
    [HarmonyPatch(typeof(PauseMenu))]
    [HarmonyPatch("drawStockRevertOptions")]
    public class PauseMenu_DrawRevertOptions
    {
        [HarmonyPostfix]
        private static void PostfixDrawStockRevertOptions(PopupDialog dialog, List<DialogGUIBase> options)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            if (FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == RevertSystem.Singleton.StartingVesselId && !VesselCommon.IsSpectating)
                return;

            foreach (var guiComponent in options)
            {
                if (guiComponent is DialogGUILabel guiLabel)
                {
                    guiLabel.OptionText = LocalizationContainer.RevertDialogText.CannotRevertText;
                }

                if (guiComponent is DialogGUIButton guiButton)
                {
                    guiButton.OptionInteractableCondition = () => false;
                }
            }
        }
    }
}
