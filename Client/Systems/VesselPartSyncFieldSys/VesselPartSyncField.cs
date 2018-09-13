using LunaClient.VesselUtilities;
using LunaCommon.Enums;
using System;
using UnityEngine;

namespace LunaClient.Systems.VesselPartSyncFieldSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselPartSyncField
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;

        public uint PartFlightId;
        public string ModuleName;
        public string FieldName;

        public PartSyncFieldType FieldType;

        public string StrValue;
        public bool BoolValue;
        public int IntValue;
        public float FloatValue;
        public double DoubleValue;
        public Vector3 VectorValue;
        public Quaternion QuaternionValue;

        #endregion

        public void ProcessPartMethodSync()
        {
            var vessel = FlightGlobals.FindVessel(VesselId);
            if (vessel == null) return;

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
                            {
                                VesselPartModuleAccess.SetPartModuleFieldValue(VesselId, part.flightID, ModuleName, FieldName, BoolValue);
                            }
                            break;
                        case PartSyncFieldType.Integer:
                            module.moduleValues.SetValue(FieldName, IntValue);
                            if (module.moduleRef != null)
                            {
                                VesselPartModuleAccess.SetPartModuleFieldValue(VesselId, part.flightID, ModuleName, FieldName, IntValue);
                            }
                            break;
                        case PartSyncFieldType.Float:
                            module.moduleValues.SetValue(FieldName, FloatValue);
                            if (module.moduleRef != null)
                            {
                                VesselPartModuleAccess.SetPartModuleFieldValue(VesselId, part.flightID, ModuleName, FieldName, FloatValue);
                            }
                            break;
                        case PartSyncFieldType.Double:
                            module.moduleValues.SetValue(FieldName, DoubleValue);
                            if (module.moduleRef != null)
                            {
                                VesselPartModuleAccess.SetPartModuleFieldValue(VesselId, part.flightID, ModuleName, FieldName, DoubleValue);
                            }
                            break;
                        case PartSyncFieldType.Vector3:
                            module.moduleValues.SetValue(FieldName, VectorValue);
                            if (module.moduleRef != null)
                            {
                                VesselPartModuleAccess.SetPartModuleFieldValue(VesselId, part.flightID, ModuleName, FieldName, VectorValue);
                            }
                            break;
                        case PartSyncFieldType.Quaternion:
                            module.moduleValues.SetValue(FieldName, QuaternionValue);
                            if (module.moduleRef != null)
                            {
                                VesselPartModuleAccess.SetPartModuleFieldValue(VesselId, part.flightID, ModuleName, FieldName, QuaternionValue);
                            }
                            break;
                        case PartSyncFieldType.String:
                            module.moduleValues.SetValue(FieldName, StrValue);
                            if (module.moduleRef != null)
                            {
                                VesselPartModuleAccess.SetPartModuleFieldValue(VesselId, part.flightID, ModuleName, FieldName, StrValue);
                            }
                            break;
                        case PartSyncFieldType.Object:
                            module.moduleValues.SetValue(FieldName, StrValue);
                            //We do not set the value of objects in the module as we cannot be sure if they can be transformed from a string back to the object
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}
