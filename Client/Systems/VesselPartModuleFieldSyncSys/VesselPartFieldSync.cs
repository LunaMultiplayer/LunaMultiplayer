using LunaClient.ModuleStore;
using LunaClient.VesselUtilities;
using System;

namespace LunaClient.Systems.VesselPartModuleFieldSyncSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselPartFieldSync
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;

        public uint PartFlightId;
        public string ModuleName;
        public string BaseModuleName;
        public string FieldName;
        public string Value;

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
                    if (module.moduleRef != null)
                    {
                        UpdateVesselModule(module, part);
                    }
                }
            }
        }

        private void UpdateVesselModule(ProtoPartModuleSnapshot module, ProtoPartSnapshot part)
        {
            if (FieldModuleStore.ModuleFieldsDictionary.TryGetValue(BaseModuleName, out var moduleDef))
            {
                if (moduleDef.PersistentModuleField.TryGetValue(FieldName, out var fieldDef))
                {
                    var convertedVal = Convert.ChangeType(Value, fieldDef.FieldType);
                    if (convertedVal != null) fieldDef.SetValue(module.moduleRef, convertedVal);
                }
            }

            module.moduleRef.Load(module.moduleValues);
            module.moduleRef.OnAwake();
            module.moduleRef.OnLoad(module.moduleValues);
            module.moduleRef.OnStart(part.partRef.GetModuleStartState());
        }
    }
}
