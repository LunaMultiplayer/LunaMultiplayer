using Harmony;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using System;

namespace LunaClient.Systems.VesselPartModuleMethodSyncSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselPartMethodSync
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;

        public uint PartFlightId;
        public string ModuleName;
        public string MethodName;

        public int FieldCount;
        public FieldNameValue[] FieldValues = new FieldNameValue[0];

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
                    foreach (var field in FieldValues)
                    {
                        module.moduleValues.SetValue(field.FieldName, field.Value);
                    }

                    module.moduleRef?.GetType().GetMethod(MethodName, AccessTools.all)?.Invoke(module.moduleRef, null);
                }
            }
        }
    }
}
