using LunaClient.VesselUtilities;
using System;

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
        public string Value;
        
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
                    module.moduleValues.SetValue(FieldName, Value);
                    if (module.moduleRef != null)
                    {
                        VesselPartModuleAccess.SetPartModuleFieldValue(VesselId, part.flightID, ModuleName, FieldName,Value);
                    }
                }
            }
        }
    }
}
