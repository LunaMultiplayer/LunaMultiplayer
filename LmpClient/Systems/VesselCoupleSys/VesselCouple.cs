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

        public bool ProcessCouple()
        {
            return ProcessCoupleInternal(VesselId, CoupledVesselId, PartFlightId, CoupledPartFlightId);
        }

        public static bool ProcessCouple(VesselCoupleMsgData msgData)
        {
            return ProcessCoupleInternal(msgData.VesselId, msgData.CoupledVesselId, msgData.PartFlightId, msgData.CoupledPartFlightId);
        }
        
        private static bool ProcessCoupleInternal(Guid vesselId, Guid coupledVesselId, uint partFlightId, uint coupledPartFlightId)
        {
            if (!VesselCommon.DoVesselChecks(vesselId))
                return false;

            //If the coupling is against our OWN vessel we must FORCE the loading
            var forceLoad = FlightGlobals.ActiveVessel && (FlightGlobals.ActiveVessel.id == vesselId || FlightGlobals.ActiveVessel.id == coupledVesselId);

            var vessel = FlightGlobals.FindVessel(vesselId);
            if (vessel == null) return false;
            if (!vessel.loaded && forceLoad) vessel.Load();

            var coupledVessel = FlightGlobals.FindVessel(coupledVesselId);
            if (coupledVessel == null) return false;
            if (!coupledVessel.loaded && forceLoad) coupledVessel.Load();

            var protoPart = VesselCommon.FindProtoPartInProtovessel(vessel.protoVessel, partFlightId);
            var coupledProtoPart = VesselCommon.FindProtoPartInProtovessel(coupledVessel.protoVessel, coupledPartFlightId);
            if (protoPart != null && coupledProtoPart != null)
            {
                if (protoPart.partRef && coupledProtoPart.partRef)
                {
                    VesselCoupleSystem.Singleton.IgnoreEvents = true;
                    protoPart.partRef.Couple(coupledProtoPart.partRef);
                    VesselCoupleSystem.Singleton.IgnoreEvents = false;

                    return true;
                }
            }

            return false;
        }
    }
}
