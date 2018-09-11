using Harmony;
using LunaClient.Events;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LunaClient.ModuleStore.Harmony
{
    public class HarmonyCustomMethod
    {
        public static readonly HarmonyMethod MethodPostfixMethod = new HarmonyMethod(typeof(HarmonyCustomMethod).GetMethod(nameof(MethodPostfix)));

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static void MethodPostfix(PartModule __instance, MethodInfo __originalMethod)
        {
            PartModuleEvent.onPartModuleMethodCalled.Fire(__instance, __originalMethod.Name);
        }
    }
}
