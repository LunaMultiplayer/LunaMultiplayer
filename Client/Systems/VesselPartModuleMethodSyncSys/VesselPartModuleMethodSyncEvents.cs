using Harmony;
using LunaClient.Base;
using LunaClient.ModuleStore;
using LunaCommon.Message.Data.Vessel;
using System.Collections.Generic;
using UniLinq;

namespace LunaClient.Systems.VesselPartModuleMethodSyncSys
{
    public class VesselPartModuleMethodSyncEvents : SubSystem<VesselPartModuleMethodSyncSystem>
    {
        public void PartModuleMethodCalled(PartModule module, string methodName)
        {
            var vessel = module.vessel;
            if (vessel == null || !vessel.loaded || vessel.protoVessel == null) return;

            var part = module.part;
            if (part == null) return;

            //The vessel is immortal so we are sure that it's not ours
            if (float.IsPositiveInfinity(part.crashTolerance))
                return;
            
            var list = new List<FieldNameValue>();
            
            if (FieldModuleStore.InheritanceTypeChain.TryGetValue(module.moduleName, out var inheritChain))
            {
                foreach (var baseModuleName in inheritChain)
                {
                    if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(baseModuleName, out var customization))
                    {
                        var methodCustomization = customization.SyncMethods.FirstOrDefault(m => m.MethodName == methodName);
                        if (methodCustomization != null)
                        {
                            foreach (var field in methodCustomization.Fields)
                            {
                                var value = module.GetType().GetField(field, AccessTools.all)?.GetValue(module).ToString();
                                list.Add(new FieldNameValue
                                {
                                    FieldName = field,
                                    Value = value
                                });
                            }
                        }
                    }
                }
            }


            LunaLog.LogError($"Part sync method {methodName} in module {module.moduleName} from part {part.flightID} was called.");
            System.MessageSender.SendVesselPartSyncMsg(vessel.id, part.flightID, module.moduleName, methodName, list.ToArray());
        }
    }
}
