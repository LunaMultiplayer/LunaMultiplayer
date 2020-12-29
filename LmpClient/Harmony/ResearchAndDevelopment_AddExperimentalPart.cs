using System.Collections.Generic;
using HarmonyLib;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when adding an experimental part
    /// </summary>
    [HarmonyPatch(typeof(ResearchAndDevelopment))]
    [HarmonyPatch("AddExperimentalPart")]
    public class ResearchAndDevelopment_AddExperimentalPart
    {
        [HarmonyPrefix]
        private static void PrefixAddExperimentalPart(AvailablePart ap, out int __state)
        {
            //We use __state to store the part counts before and compare it against the count after removing the experimental part
            //This way we only trigger the event if there's a real change in the experimental part counts
            __state = -1;

            if (ResearchAndDevelopment.Instance == null) return;

            var experimentalParts = Traverse.Create(ResearchAndDevelopment.Instance).Field<Dictionary<AvailablePart, int>>("experimentalPartsStock").Value;
            if (experimentalParts != null)
            {
                __state = experimentalParts.ContainsKey(ap) ? experimentalParts[ap] : 0;
            }
        }

        [HarmonyPostfix]
        private static void PostfixAddExperimentalPart(AvailablePart ap, int __state)
        {
            if (ResearchAndDevelopment.Instance == null) return;

            var experimentalParts = Traverse.Create(ResearchAndDevelopment.Instance).Field<Dictionary<AvailablePart, int>>("experimentalPartsStock").Value;
            if (experimentalParts != null && experimentalParts.ContainsKey(ap))
            {
                if (experimentalParts[ap] != __state)
                    ExperimentalPartEvent.onExperimentalPartAdded.Fire(ap, experimentalParts[ap]);
            }
        }
    }
}
