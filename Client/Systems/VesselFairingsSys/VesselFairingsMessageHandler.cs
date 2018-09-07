using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselFairingsSys
{
    public class VesselFairingsMessageHandler : SubSystem<VesselFairingsSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselFairingMsgData msgData) || !System.FairingSystemReady) return;

            //We received a msg for our own controlled/updated vessel so ignore it
            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;
            
            var vessel = FlightGlobals.FindVessel(msgData.VesselId);
            if (vessel == null) return;

            var part = VesselCommon.FindProtoPartInProtovessel(vessel.protoVessel, msgData.PartFlightId);
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
