using Harmony;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using System;

namespace LunaClient.Systems.VesselPartSyncSys
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
        public string MethodName;

        public int FieldCount;
        public FieldNameValue[] FieldValues = new FieldNameValue[0];

        public bool IsAction;
        public int ActionGroup;
        public int ActionType;

        private KSPActionParam ActionParam => new KSPActionParam((KSPActionGroup)ActionGroup, (KSPActionType)ActionType);

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
                    for (var i = 0; i < FieldCount; i++)
                    {
                        module.moduleValues.SetValue(FieldValues[i].FieldName, FieldValues[i].Value);
                    }
                    
                    if (module.moduleRef != null)
                    {
                        module.moduleRef.GetType().GetMethod(MethodName, AccessTools.all)?.Invoke(module.moduleRef, IsAction ? new object[] { ActionParam } : null);
                        for (var i = 0; i < FieldCount; i++)
                        {
                            var fieldToSet = module.moduleRef?.GetType().GetField(FieldValues[i].FieldName, AccessTools.all);
                            if (fieldToSet != null)
                            {
                                var convertedVal = fieldToSet.FieldType.IsEnum ? Enum.Parse(fieldToSet.FieldType, FieldValues[i].Value) :
                                    Convert.ChangeType(FieldValues[i].Value, fieldToSet.FieldType);

                                fieldToSet.SetValue(module.moduleRef, convertedVal);
                            }
                        }
                    }
                }
            }
        }
    }
}
