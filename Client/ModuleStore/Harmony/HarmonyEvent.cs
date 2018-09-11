using Harmony;
using LunaClient.Events;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LunaClient.ModuleStore.Harmony
{
    public class HarmonyEvent
    {
        public static readonly HarmonyMethod EventPostfixMethod = new HarmonyMethod(typeof(HarmonyEvent).GetMethod(nameof(EventPostfix)));

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static void EventPostfix(PartModule __instance, MethodInfo __originalMethod)
        {
            PartModuleEvent.onPartModuleEventCalled.Fire(__instance, __originalMethod.Name);
        }
    }
}
