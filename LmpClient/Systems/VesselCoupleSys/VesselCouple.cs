using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using System;

namespace LmpClient.Systems.VesselCoupleSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselCouple
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;
        public Guid CoupledVesselId;
        public uint PartFlightId;
        public uint CoupledPartFlightId;

        #endregion

        public void ProcessCouple()
        {
            ProcessCoupleInternal(VesselId, CoupledVesselId, PartFlightId, CoupledPartFlightId);
        }

        public static void ProcessCouple(VesselCoupleMsgData msgData)
        {
            ProcessCoupleInternal(msgData.VesselId, msgData.CoupledVesselId, msgData.PartFlightId, msgData.CoupledPartFlightId);
        }

        private static void ProcessCoupleInternal(Guid vesselId, Guid coupledVesselId, uint partFlightId, uint coupledPartFlightId)
        {
            if (!VesselCommon.DoVesselChecks(vesselId))
                return;

            var vessel = FlightGlobals.FindVessel(vesselId);
            if (vessel == null) return;

            var coupledVessel = FlightGlobals.FindVessel(coupledVesselId);
            if (coupledVessel == null) return;

            var protoPart = VesselCommon.FindProtoPartInProtovessel(vessel.protoVessel, partFlightId);
            var coupledProtoPart = VesselCommon.FindProtoPartInProtovessel(coupledVessel.protoVessel, coupledPartFlightId);
            if (protoPart != null && coupledProtoPart != null)
            {
                if (protoPart.partRef && coupledProtoPart.partRef)
                {
                    protoPart.partRef.Couple(coupledProtoPart.partRef);
                }
            }
        }
    }
}
