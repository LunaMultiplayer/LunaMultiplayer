using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselUtilities;

namespace LunaClient.Systems.VesselCrewSys
{
    public class VesselCrewEvents : SubSystem<VesselCrewSystem>
    {
        /// <summary>
        /// Event triggered when a kerbal boards a vessel
        /// </summary>
        public void OnCrewBoard(GameEvents.FromToAction<Part, Part> partAction)
        {
            LunaLog.Log("Crew boarding detected!");
            if (!VesselCommon.IsSpectating)
            {
                var kerbalVessel = partAction.from.vessel;
                var vessel = partAction.to.vessel;

                LunaLog.Log($"EVA Boarding. Kerbal: {kerbalVessel.id} ({kerbalVessel.vesselName}) boarding: {vessel.id} ({vessel.vesselName})");

                VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(kerbalVessel.id);
                VesselRemoveSystem.Singleton.AddToKillList(kerbalVessel.id, "Killing kerbal as it boarded a vessel");
                LockSystem.Singleton.ReleaseAllVesselLocks(new[] { kerbalVessel.vesselName }, kerbalVessel.id);

                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel, false);
            }
        }

        /// <summary>
        /// The vessel has changed as it has less crew now so send the definition
        /// </summary>
        public void OnCrewTransfered(GameEvents.HostedFromToAction<ProtoCrewMember, Part> data)
        {
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(data.from.vessel, false);
        }

        /// <summary>
        /// The vessel has changed as it has less crew now so send the definition.
        /// Also send the definition of the EVA
        /// </summary>
        public void OnCrewEva(GameEvents.FromToAction<Part, Part> data)
        {
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(data.from.vessel, false);
            //Do not send the kerbal as his orbit is not ready. It will be handled by the VesselLockEvents.OnVesselChange
        }
    }
}
