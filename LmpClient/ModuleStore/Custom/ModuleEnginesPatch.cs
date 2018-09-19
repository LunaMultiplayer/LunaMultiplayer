using Harmony;
using LunaClient.Events;
using System.Collections.Generic;
// ReSharper disable All
#pragma warning disable IDE1006 // Naming Styles

namespace LunaClient.ModuleStore.Custom
{
    public class ModuleEngines_Patch
    {
        private static bool staged;
        private static bool flameout;
        private static bool EngineIgnited;
        private static float currentThrottle;
        private static bool engineShutdown;
        private static float thrustPercentage;
        private static bool manuallyOverridden;
        public static List<bool> gimbalsList = new List<bool>();

        [HarmonyPrefix]
        public static void Shutdown_Prefix(ModuleEngines __instance)
        {
            staged = __instance.staged;
            flameout = __instance.flameout;
            EngineIgnited = __instance.EngineIgnited;
            engineShutdown = __instance.engineShutdown;
            currentThrottle = __instance.currentThrottle;
            thrustPercentage = __instance.thrustPercentage;
            manuallyOverridden = __instance.manuallyOverridden;
        }

        [HarmonyPostfix]
        public static void Shutdown_Postfix(ModuleEngines __instance)
        {
            if (staged != __instance.staged)
            {
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, nameof(staged), __instance.staged);
            }
            if (flameout != __instance.flameout)
            {
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, nameof(flameout), __instance.flameout);
            }
            if (EngineIgnited != __instance.EngineIgnited)
            {
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, nameof(EngineIgnited), __instance.EngineIgnited);
            }
            if (engineShutdown != __instance.engineShutdown)
            {
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, nameof(engineShutdown), __instance.engineShutdown);
            }
            if (currentThrottle != __instance.currentThrottle)
            {
                PartModuleEvent.onPartModuleFloatFieldChanged.Fire(__instance, nameof(currentThrottle), __instance.currentThrottle);
            }
            if (thrustPercentage != __instance.thrustPercentage)
            {
                PartModuleEvent.onPartModuleFloatFieldChanged.Fire(__instance, nameof(thrustPercentage), __instance.thrustPercentage);
            }
            if (manuallyOverridden != __instance.manuallyOverridden)
            {
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, nameof(manuallyOverridden), __instance.manuallyOverridden);
            }
        }

        //public void ShutdownAction(KSPActionParam param)
        //{
        //    //No need to patch anything as this event calls Shutdown()
        //}

        [HarmonyPrefix]
        public void Activate_Prefix(ModuleEngines __instance)
        {
            gimbalsList.Clear();
            staged = __instance.staged;
            flameout = __instance.flameout;
            EngineIgnited = __instance.EngineIgnited;
            engineShutdown = __instance.engineShutdown;
            currentThrottle = __instance.currentThrottle;
            thrustPercentage = __instance.thrustPercentage;
            manuallyOverridden = __instance.manuallyOverridden;

            if (!staged)
            {
                var gimbals = __instance.part.FindModulesImplementing<ModuleGimbal>();
                {
                    foreach (var gimbal in gimbals)
                    {
                        gimbalsList.Add(gimbal.gimbalActive);
                    }
                }
            }
        }

        [HarmonyPostfix]
        public static void Activate_Postfix(ModuleEngines __instance)
        {
            if (staged != __instance.staged)
            {
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, nameof(staged), __instance.staged);
            }
            if (flameout != __instance.flameout)
            {
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, nameof(flameout), __instance.flameout);
            }
            if (EngineIgnited != __instance.EngineIgnited)
            {
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, nameof(EngineIgnited), __instance.EngineIgnited);
            }
            if (engineShutdown != __instance.engineShutdown)
            {
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, nameof(engineShutdown), __instance.engineShutdown);
            }
            if (currentThrottle != __instance.currentThrottle)
            {
                PartModuleEvent.onPartModuleFloatFieldChanged.Fire(__instance, nameof(currentThrottle), __instance.currentThrottle);
            }
            if (thrustPercentage != __instance.thrustPercentage)
            {
                PartModuleEvent.onPartModuleFloatFieldChanged.Fire(__instance, nameof(thrustPercentage), __instance.thrustPercentage);
            }
            if (manuallyOverridden != __instance.manuallyOverridden)
            {
                PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, nameof(manuallyOverridden), __instance.manuallyOverridden);
            }

            if (!staged)
            {
                var gimbals = __instance.part.FindModulesImplementing<ModuleGimbal>();
                {
                    for (var i = 0; i < gimbals.Count; i++)
                    {
                        if (gimbalsList[i] != gimbals[i].gimbalActive)
                        {
                            PartModuleEvent.onPartModuleBoolFieldChanged.Fire(__instance, "gimbalActive", gimbals[i].gimbalActive);
                        }
                    }
                }
            }
        }

        //public void ActivateAction(KSPActionParam param)
        //{
        //    //No need to patch anything as this event calls Activate()
        //}
    }
}
