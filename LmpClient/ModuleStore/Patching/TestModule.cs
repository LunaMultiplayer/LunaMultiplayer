using System.Reflection;
using HarmonyLib;
using LmpClient.Events;

namespace LmpClient.ModuleStore.Patching
{
    /// <summary>
    /// This is a test class, we use the method "ActionCallMethodInfo/EventCallMethodInfo/MethodCallMethodInfo" to take the IL codes and put them on the real part module methods
    /// </summary>
    internal class TestModule : PartModule
    {
        public static readonly MethodInfo AfterMethodCallMethodInfo = typeof(TestModule).GetMethod(nameof(AfterMethodCall), AccessTools.all);

        private void AfterMethodCall() => PartModuleEvent.onPartModuleMethodCalling.Fire(this, "METHODNAME");
    }
}
