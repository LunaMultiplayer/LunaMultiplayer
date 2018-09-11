using Harmony;
using LunaClient.Events;
using System.Reflection;

namespace LunaClient.ModuleStore.Patching
{
    /// <summary>
    /// This is a test class, we use the method "ActionCallMethodInfo/EventCallMethodInfo/MethodCallMethodInfo" to take the IL codes and put them on the real part module methods
    /// </summary>
    internal class TestModule : PartModule
    {
        public static readonly MethodInfo AfterActionCallMethodInfo = typeof(TestModule).GetMethod(nameof(AfterActionCall), AccessTools.all);
        public static readonly MethodInfo AfterEventCallMethodInfo = typeof(TestModule).GetMethod(nameof(AfterEventCall), AccessTools.all);
        public static readonly MethodInfo AfterMethodCallMethodInfo = typeof(TestModule).GetMethod(nameof(AfterMethodCall), AccessTools.all);

        [KSPEvent(guiName = "AfterActionCall")]
        private void AfterActionCall(KSPActionParam param) => PartModuleEvent.onPartModuleActionCalled.Fire(this, "METHODNAME", param);

        [KSPEvent(guiName = "AfterEventCall")]
        private void AfterEventCall() => PartModuleEvent.onPartModuleEventCalled.Fire(this, "METHODNAME");

        private void AfterMethodCall() => PartModuleEvent.onPartModuleMethodCalled.Fire(this, "METHODNAME");
    }
}
