using Harmony;
using KSP.UI.Dialogs;
using LmpClient.Localization;
using LmpClient.Systems.Revert;
using LmpClient.VesselUtilities;
using LmpCommon.Enums;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to disable the revert after switching vessels
    /// </summary>
    [HarmonyPatch(typeof(FlightResultsDialog))]
    [HarmonyPatch("SetupGUI")]
    public class FlightResultsDialog_SetupGUI
    {
        [HarmonyPostfix]
        private static void PostfixSetupGUI(Button ___Btn_revLaunch, Button ___Btn_revEditor)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            if (FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == RevertSystem.Singleton.StartingVesselId && !VesselCommon.IsSpectating)
                return;

            ___Btn_revLaunch.onClick.RemoveAllListeners();
            ___Btn_revEditor.onClick.RemoveAllListeners();

            ___Btn_revLaunch.onClick.AddListener(() =>
            {
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "CannotRevertLaunch",
                    LocalizationContainer.RevertDialogText.CannotRevertTitle,
                    LocalizationContainer.RevertDialogText.CannotRevertText,
                    LocalizationContainer.RevertDialogText.CloseBtn, false, HighLogic.UISkin);
            });

            ___Btn_revEditor.onClick.AddListener(() =>
            {
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "CannotRevertEditor",
                    LocalizationContainer.RevertDialogText.CannotRevertTitle,
                    LocalizationContainer.RevertDialogText.CannotRevertText,
                    LocalizationContainer.RevertDialogText.CloseBtn, false, HighLogic.UISkin);
            });
        }
    }
}
