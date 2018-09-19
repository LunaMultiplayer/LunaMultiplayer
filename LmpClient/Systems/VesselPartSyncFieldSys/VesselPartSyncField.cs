using LmpClient.Extensions;
using LmpClient.VesselUtilities;
using LmpCommon.Enums;
using System;
using UnityEngine;

namespace LmpClient.Systems.VesselPartSyncFieldSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselPartSyncField
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;
        public uint VesselPersistentId;

        public uint PartFlightId;
        public uint PartPersistentId;
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
            var vessel = FlightGlobals.fetch.FindVessel(VesselPersistentId, VesselId);
            if (vessel == null) return;

            var part = VesselCommon.FindProtoPartInProtovessel(PartPersistentId, vessel.protoVessel, PartFlightId);
            if (part != null)
            {
                var module = VesselCommon.FindProtoPartModuleInProtoPart(part, ModuleName);
                if (module != null)
                {
                    switch (FieldType)
                    {
                        case PartSyncFieldType.Boolean:
                            module.moduleValues.SetValue(FieldName, BoolValue);
                            break;
                        case PartSyncFieldType.Integer:
                            module.moduleValues.SetValue(FieldName, IntValue);
                            break;
                        case PartSyncFieldType.Float:
                            module.moduleValues.SetValue(FieldName, FloatValue);
                            break;
                        case PartSyncFieldType.Double:
                            module.moduleValues.SetValue(FieldName, DoubleValue);
                            break;
                        case PartSyncFieldType.Vector3:
                            module.moduleValues.SetValue(FieldName, VectorValue);
                            break;
                        case PartSyncFieldType.Quaternion:
                            module.moduleValues.SetValue(FieldName, QuaternionValue);
                            break;
                        case PartSyncFieldType.String:
                            module.moduleValues.SetValue(FieldName, StrValue);
                            break;
                        case PartSyncFieldType.Enum:
                            module.moduleValues.SetValue(FieldName, StrValue);
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
