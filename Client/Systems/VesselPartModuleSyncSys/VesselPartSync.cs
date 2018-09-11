using LunaClient.ModuleStore;
using LunaClient.VesselUtilities;
using System;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselPartSync
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;

        public uint PartFlightId;
        public string ModuleName;
        public string BaseModuleName;
        public string FieldName;
        public string Value;
        public bool ServerIgnore;

        #endregion

        public void ProcessPartSync()
        {
            var vessel = FlightGlobals.FindVessel(VesselId);
            if (vessel == null) return;

            var part = VesselCommon.FindProtoPartInProtovessel(vessel.protoVessel, PartFlightId);
            if (part != null)
            {
                var module = VesselCommon.FindProtoPartModuleInProtoPart(part, ModuleName);
                if (module != null)
                {
                    module.moduleValues.SetValue(FieldName, Value);
                    UpdateVesselModuleIfNeeded(module, part);
                }
            }
        }

        private void UpdateVesselModuleIfNeeded(ProtoPartModuleSnapshot module, ProtoPartSnapshot part)
        {
            if (module.moduleRef == null) return;

            switch (CustomizationsHandler.SkipModule(VesselId, PartFlightId, BaseModuleName, FieldName, true, out var customization))
            {
                case CustomizationResult.TooEarly:
                case CustomizationResult.Ignore:
                    break;
                case CustomizationResult.Ok:
                    if (customization.IgnoreSpectating && FlightGlobals.ActiveVessel?.id == VesselId) break;

                    if (customization.SetValueInModule)
                    {
                        if (FieldModuleStore.ModuleFieldsDictionary.TryGetValue(BaseModuleName, out var moduleDef))
                        {
                            if (moduleDef.PersistentModuleField.TryGetValue(FieldName, out var fieldDef))
                            {
                                var convertedVal = Convert.ChangeType(Value, fieldDef.FieldType);
                                if (convertedVal != null) fieldDef.SetValue(module.moduleRef, convertedVal);
                            }
                        }
                    }

                    if (customization.CallLoad)
                        module.moduleRef.Load(module.moduleValues);
                    if (customization.CallOnAwake)
                        module.moduleRef.OnAwake();
                    if (customization.CallOnLoad)
                        module.moduleRef.OnLoad(module.moduleValues);
                    if (customization.CallOnStart)
                        module.moduleRef.OnStart(part.partRef.GetModuleStartState());
                    break;
            }
        }
    }
}
