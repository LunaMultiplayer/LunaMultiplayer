using HarmonyLib;
using LmpClient.Events;
// ReSharper disable All

namespace LmpClient.ModuleStore.Harmony
{
    [HarmonyPatch(typeof(ModuleScienceExperiment))]
    [HarmonyPatch("resetExperiment")]
    public class ModuleScienceExperiment_ResetExperiment
    {
        [HarmonyPostfix]
        private static void PostfixResetExperiment(ModuleScienceExperiment __instance)
        {
            ExperimentEvent.onExperimentReset.Fire(__instance.vessel);
        }
    }
}
