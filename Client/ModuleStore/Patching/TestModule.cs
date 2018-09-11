using Harmony;
using LunaClient.Events;
using System.Reflection;

namespace LunaClient.ModuleStore.Patching
{
    /// <summary>
    /// This is a test class, we use the method "ExampleFieldChangeCall/ExampleMethodCall" to take the IL codes and paste them on the real part module methods
    /// </summary>
    internal class TestModule : PartModule
    {
        public static readonly MethodInfo ExampleFieldChangeCallMethod = typeof(TestModule).GetMethod(nameof(ExampleFieldChangeCall), AccessTools.all);
        public static readonly MethodInfo ExampleMethodCallMethod = typeof(TestModule).GetMethod(nameof(ExampleMethodCall), AccessTools.all);
        private void ExampleFieldChangeCall() => PartModuleEvent.onPartModuleFieldChange.Fire(this, "FIELDNAME");
        private void ExampleMethodCall()
        {
            PartModuleEvent.onPartModuleMethodCalled.Fire(this, "METHODNAME", new object[0]);
        }
        
        private void ExampleMethodCall1(int par1)
        {
            PartModuleEvent.onPartModuleMethodCalled.Fire(this, "METHODNAME", new object[] { par1 });
        }
        private void ExampleMethodCall2(int par1, string par2)
        {
            PartModuleEvent.onPartModuleMethodCalled.Fire(this, "METHODNAME", new object[] { par1, par2 });
        }

        private void ExampleMethodCall2(int par1, string par2, KSPActionParam par3)
        {
            PartModuleEvent.onPartModuleMethodCalled.Fire(this, "METHODNAME", new object[] { par1, par2, par3 });
        }
    }
}
