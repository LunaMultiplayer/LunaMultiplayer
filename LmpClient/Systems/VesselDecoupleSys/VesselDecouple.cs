using LmpClient.Systems.VesselPositionSys;
using LmpClient.VesselUtilities;
using System;

namespace LmpClient.Systems.VesselDecoupleSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselDecouple
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;
        public uint PartFlightId;
        public float BreakForce;
        public Guid NewVesselId;

        #endregion

        public void ProcessDecouple()
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
                    VesselDecoupleSystem.Singleton.IgnoreEvents = true;
                    protoPart.partRef.decouple(BreakForce);
                    protoPart.partRef.vessel.id = NewVesselId;
                    VesselPositionSystem.Singleton.ForceUpdateVesselPosition(NewVesselId);
                    VesselDecoupleSystem.Singleton.IgnoreEvents = false;
                }
            }
        }
    }
}
