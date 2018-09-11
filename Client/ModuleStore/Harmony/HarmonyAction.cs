using Harmony;
using LunaClient.Events;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LunaClient.ModuleStore.Harmony
{
    public class HarmonyAction
    {
        public static readonly HarmonyMethod ActionPostfixMethod = new HarmonyMethod(typeof(HarmonyAction).GetMethod(nameof(ActionPostfix)));

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static void ActionPostfix(PartModule __instance, MethodInfo __originalMethod, KSPActionParam param)
        {
            PartModuleEvent.onPartModuleActionCalled.Fire(__instance, __originalMethod.Name, param);
        }
    }
}
