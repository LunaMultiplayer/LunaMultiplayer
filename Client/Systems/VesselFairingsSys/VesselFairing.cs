using LunaClient.VesselUtilities;
using System;

namespace LunaClient.Systems.VesselFairingsSys
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
            var vessel = FlightGlobals.FindVessel(VesselId);
            if (vessel == null) return;

            var part = VesselCommon.FindProtoPartInProtovessel(vessel.protoVessel, PartFlightId);
            if (part != null)
            {
                var module = VesselCommon.FindProtoPartModuleInProtoPart(part, "ModuleProceduralFairing");
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
}
