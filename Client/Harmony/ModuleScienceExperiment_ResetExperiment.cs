using Harmony;
using LunaClient.Events;
// ReSharper disable All

namespace LunaClient.Harmony
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
