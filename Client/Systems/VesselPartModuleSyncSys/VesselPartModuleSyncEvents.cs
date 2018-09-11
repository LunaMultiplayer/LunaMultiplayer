using LunaClient.Base;
using LunaClient.Extensions;
using LunaClient.ModuleStore;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    public class VesselPartModuleSyncEvents : SubSystem<VesselPartModuleSyncSystem>
    {
        public void PartModuleFieldChange(PartModule module, string fieldName)
        {
            var vessel = module.vessel;
            if (vessel == null || !vessel.loaded || vessel.protoVessel == null) return;

            var part = module.part;
            if (part == null) return;

            //The vessel is immortal so we are sure that it's not ours
            if (float.IsPositiveInfinity(part.crashTolerance))
                return;

            if (FieldModuleStore.InheritanceTypeChain.TryGetValue(module.moduleName, out var inheritChain))
            {
                foreach (var baseModuleName in inheritChain)
                {
                    if (FieldModuleStore.ModuleFieldsDictionary.TryGetValue(baseModuleName, out var definition))
                    {
                        if (definition.PersistentModuleField.TryGetValue(fieldName, out var fieldInfo))
                        {
                            var customizationResult = CustomizationsHandler.SkipModule(vessel.id, part.flightID, baseModuleName, fieldInfo.Name, false, out _);
                            if (customizationResult != CustomizationResult.Ok)
                                return;

                            var fieldVal = fieldInfo.GetValue(module).ToInvariantString();
                            var snapshotVal = module.snapshot?.moduleValues.GetValue(fieldInfo.Name);

                            if (snapshotVal != null && fieldVal != null && fieldVal != snapshotVal)
                            {
                                LunaLog.Log($"Detected a part module change. FlightId: {part.flightID} PartName: {part.name} Module: {module.moduleName} BaseModule: {baseModuleName} " +
                                            $"Field: {fieldInfo.Name} ValueBefore: {snapshotVal} ValueNow: {fieldVal}");

                                System.MessageSender.SendVesselPartSyncMsg(vessel.id, part.flightID, module.moduleName, baseModuleName, fieldInfo.Name, fieldVal);
                            }
                        }
                    }
                }
            }
        }
    }
}
