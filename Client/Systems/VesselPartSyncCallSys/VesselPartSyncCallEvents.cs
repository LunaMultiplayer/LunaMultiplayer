using LunaClient.Base;
using LunaClient.ModuleStore;
using UniLinq;

namespace LunaClient.Systems.VesselPartSyncCallSys
{
    public class VesselPartSyncCallEvents : SubSystem<VesselPartSyncCallSystem>
    {
        private static bool CallIsValid(PartModule module)
        {
            var vessel = module.vessel;
            if (vessel == null || !vessel.loaded || vessel.protoVessel == null)
                return false;

            var part = module.part;
            if (part == null)
                return false;

            //The vessel is immortal so we are sure that it's not ours
            if (float.IsPositiveInfinity(part.crashTolerance))
                return false;

            return true;
        }

        public void PartModuleMethodCalled(PartModule module, string methodName)
        {
            if (!CallIsValid(module))
                return;

            if (FieldModuleStore.InheritanceTypeChain.TryGetValue(module.moduleName, out var inheritChain))
            {
                foreach (var baseModuleName in inheritChain)
                {
                    if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(baseModuleName, out var customization))
                    {
                        var methodCustomization = customization.Methods.FirstOrDefault(m => m.MethodName == methodName);
                    }
                }
            }

            LunaLog.Log($"Part sync method {methodName} in module {module.moduleName} from part {module.part.flightID} was called.");
            System.MessageSender.SendVesselPartSyncCallMsg(module.vessel, module.part, module.moduleName, methodName);
        }
    }
}
