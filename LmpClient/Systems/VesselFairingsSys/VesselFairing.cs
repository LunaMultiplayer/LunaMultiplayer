using LmpClient.Extensions;
using LmpClient.VesselUtilities;
using System;

namespace LmpClient.Systems.VesselFairingsSys
{
    /// <summary>
    /// Class that maps a message class to a system class. This way we avoid the message caching issues
    /// </summary>
    public class VesselFairing
    {
        #region Fields and Properties

        public double GameTime;
        public Guid VesselId;
        public uint PartFlightId;

        #endregion

        public void ProcessFairing()
        {
            if (!VesselCommon.DoVesselChecks(VesselId))
                return;

            //Finding using persistentId failed, try searching it with the flightId...
            var vessel = FlightGlobals.fetch.LmpFindVessel(VesselId);
            if (vessel == null) return;

            var protoPart = VesselCommon.FindProtoPartInProtovessel(vessel.protoVessel, PartFlightId);
            if (protoPart != null)
            {
                ProcessFairingChange(protoPart);
            }
        }

        private static void ProcessFairingChange(ProtoPartSnapshot protoPart)
        {
            var module = VesselCommon.FindProtoPartModuleInProtoPart(protoPart, "ModuleProceduralFairing");
            module?.moduleValues.SetValue("fsm", "st_flight_deployed");
            module?.moduleValues.RemoveNodesStartWith("XSECTION");

            try
            {
                (module?.moduleRef as ModuleProceduralFairing)?.DeployFairing();
            }
            catch (Exception)
            {
                //TODO reload the module
            }
        }
    }
}
