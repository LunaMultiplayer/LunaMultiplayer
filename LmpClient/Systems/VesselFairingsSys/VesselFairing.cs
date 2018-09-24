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
        public uint VesselPersistentId;

        public uint PartFlightId;
        public uint PartPersistentId;

        #endregion

        public void ProcessFairing()
        {
            if (!VesselCommon.DoVesselChecks(VesselId))
                return;

            if (FlightGlobals.FindUnloadedPart(PartPersistentId, out var protoPart))
            {
                ProcessFairingChange(protoPart);
            }
            else
            {
                //Finding using persistentId failed, try searching it with the flightId...
                var vessel = FlightGlobals.fetch.FindVessel(VesselPersistentId, VesselId);
                if (vessel == null) return;

                var part = VesselCommon.FindProtoPartInProtovessel(PartPersistentId, vessel.protoVessel, PartFlightId);
                if (part != null)
                {
                    ProcessFairingChange(protoPart);
                }
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
