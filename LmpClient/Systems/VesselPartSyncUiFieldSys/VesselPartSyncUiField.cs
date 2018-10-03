using System;
using LmpClient.Extensions;
using LmpClient.VesselUtilities;
using LmpCommon.Enums;

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
                            module.moduleRef?.Fields[FieldName].SetValue(BoolValue, module.moduleRef);
                            break;
                        case PartSyncFieldType.Integer:
                            module.moduleValues.SetValue(FieldName, IntValue);
                            module.moduleRef?.Fields[FieldName].SetValue(IntValue, module.moduleRef);
                            break;
                        case PartSyncFieldType.Float:
                            module.moduleValues.SetValue(FieldName, FloatValue);
                            module.moduleRef?.Fields[FieldName].SetValue(FloatValue, module.moduleRef);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}
