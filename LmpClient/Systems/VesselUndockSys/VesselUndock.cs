using LmpClient.VesselUtilities;
using System;

namespace LmpClient.Systems.VesselUndockSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselUndock
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;

        public uint PartFlightId;
        public Guid NewVesselId;

        public DockedVesselInfo DockedInfo;

        #endregion

        public void ProcessUndock()
        {
            if (!VesselCommon.DoVesselChecks(VesselId))
                return;

            var vessel = FlightGlobals.FindVessel(VesselId);
            if (vessel == null) return;

            var protoPart = VesselCommon.FindProtoPartInProtovessel(vessel.protoVessel, PartFlightId);
            if (protoPart != null)
            {
                if (protoPart.partRef)
                {
                    protoPart.partRef.Undock(DockedInfo);
                    protoPart.partRef.vessel.id = NewVesselId;
                }
            }
        }
    }
}
