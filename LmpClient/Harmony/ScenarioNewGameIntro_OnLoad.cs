using HarmonyLib;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// LMP rebuilds the game from scratch on every server connection and blocks
    /// ScenarioNewGameIntro from server sync (IgnoredScenarios), so the tutorial
    /// flags are always false on load. This causes the KSC welcome popup and VAB/
    /// tracking station tutorials to trigger every session.
    ///
    /// Fix: prefix that sets all tutorial flags to true and skips stock OnLoad,
    /// unconditionally suppressing all tutorials in multiplayer.
    /// </summary>
    [HarmonyPatch(typeof(ScenarioNewGameIntro))]
    [HarmonyPatch("OnLoad")]
    public class ScenarioNewGameIntro_OnLoad
    {
        [HarmonyPrefix]
        private static bool PrefixOnLoad(ScenarioNewGameIntro __instance)
        {
            __instance.kscComplete = true;
            __instance.editorComplete = true;
            __instance.tsComplete = true;
            return false; // Skip stock OnLoad
        }
    }
}
