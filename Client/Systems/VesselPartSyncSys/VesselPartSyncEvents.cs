using Harmony;
using LunaClient.Base;
using LunaClient.ModuleStore;
using LunaCommon.Message.Data.Vessel;
using System.Collections.Generic;
using UniLinq;

namespace LunaClient.Systems.VesselPartSyncSys
{
    public class VesselPartSyncEvents : SubSystem<VesselPartSyncSystem>
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

        public void PartModuleEventCalled(PartModule module, string methodName)
        {
            if (!CallIsValid(module))
                return;

            var list = new List<FieldNameValue>();

            if (FieldModuleStore.InheritanceTypeChain.TryGetValue(module.moduleName, out var inheritChain))
            {
                foreach (var baseModuleName in inheritChain)
                {
                    if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(baseModuleName, out var customization))
                    {
                        var eventMethodCustomization = customization.EventMethods.FirstOrDefault(m => m.MethodName == methodName);
                        if (eventMethodCustomization != null)
                        {
                            foreach (var field in eventMethodCustomization.Fields)
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

            LunaLog.Log($"Part sync event {methodName} in module {module.moduleName} from part {module.part.flightID} was called.");
            System.MessageSender.SendVesselPartSyncMsg(module.vessel.id, module.part.flightID, module.moduleName, methodName, list.ToArray());
        }

        public void PartModuleMethodCalled(PartModule module, string methodName)
        {
            if (!CallIsValid(module))
                return;

            var list = new List<FieldNameValue>();
            if (FieldModuleStore.InheritanceTypeChain.TryGetValue(module.moduleName, out var inheritChain))
            {
                foreach (var baseModuleName in inheritChain)
                {
                    if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(baseModuleName, out var customization))
                    {
                        var methodCustomization = customization.Methods.FirstOrDefault(m => m.MethodName == methodName);
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

            LunaLog.Log($"Part sync method {methodName} in module {module.moduleName} from part {module.part.flightID} was called.");
            System.MessageSender.SendVesselPartSyncMsg(module.vessel.id, module.part.flightID, module.moduleName, methodName, list.ToArray());
        }

        public void PartModuleActionCalled(PartModule module, string methodName, KSPActionParam parameter)
        {
            if (!CallIsValid(module))
                return;

            var list = new List<FieldNameValue>();
            if (FieldModuleStore.InheritanceTypeChain.TryGetValue(module.moduleName, out var inheritChain))
            {
                foreach (var baseModuleName in inheritChain)
                {
                    if (FieldModuleStore.CustomizedModuleBehaviours.TryGetValue(baseModuleName, out var customization))
                    {
                        var methodCustomization = customization.Methods.FirstOrDefault(m => m.MethodName == methodName);
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

            LunaLog.Log($"Part sync method {methodName} in module {module.moduleName} from part {module.part.flightID} was called.");
            System.MessageSender.SendVesselPartSyncActionMsg(module.vessel.id, module.part.flightID, module.moduleName, methodName, list.ToArray(), parameter);
        }
    }
}
