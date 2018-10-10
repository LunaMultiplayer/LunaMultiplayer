using LmpClient.Events;
using LmpClient.Extensions;
using LmpClient.VesselUtilities;
using LmpCommon.Enums;
using System;

namespace LmpClient.Systems.VesselPartSyncUiFieldSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselPartSyncUiField
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;

        public uint PartFlightId;
        public string ModuleName;
        public string FieldName;

        public PartSyncFieldType FieldType;

        public bool BoolValue;
        public int IntValue;
        public float FloatValue;

        #endregion

        public void ProcessPartMethodSync()
        {
            var vessel = FlightGlobals.fetch.LmpFindVessel(VesselId);
            if (vessel == null) return;

            if (!VesselCommon.DoVesselChecks(VesselId))
                return;

            var part = VesselCommon.FindProtoPartInProtovessel(vessel.protoVessel, PartFlightId);
            if (part != null)
            {
                var module = VesselCommon.FindProtoPartModuleInProtoPart(part, ModuleName);
                if (module != null)
                {
                    switch (FieldType)
                    {
                        case PartSyncFieldType.Boolean:
                            module.moduleValues.SetValue(FieldName, BoolValue);
                            if (module.moduleRef != null)
                                module.moduleRef.Fields[FieldName].SetValue(BoolValue, module.moduleRef);
                            PartModuleEvent.onPartModuleBoolFieldProcessed.Fire(module, FieldName, BoolValue);
                            break;
                        case PartSyncFieldType.Integer:
                            module.moduleValues.SetValue(FieldName, IntValue);
                            if (module.moduleRef != null)
                                module.moduleRef.Fields[FieldName].SetValue(IntValue, module.moduleRef);
                            PartModuleEvent.onPartModuleIntFieldProcessed.Fire(module, FieldName, IntValue);
                            break;
                        case PartSyncFieldType.Float:
                            module.moduleValues.SetValue(FieldName, FloatValue);
                            if (module.moduleRef != null)
                                module.moduleRef.Fields[FieldName].SetValue(FloatValue, module.moduleRef);
                            PartModuleEvent.onPartModuleFloatFieldProcessed.Fire(module, FieldName, FloatValue);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}
