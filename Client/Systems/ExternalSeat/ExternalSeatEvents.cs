using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using System;

namespace LunaClient.Systems.ExternalSeat
{
    public class ExternalSeatEvents : SubSystem<ExternalSeatSystem>
    {
        public void ExternalSeatBoard(KerbalSeat seat, Guid kerbalVesselId, string kerbalName)
        {
            if (seat.vessel == null) return;

            LunaLog.Log("Crew-board to an external seat detected!");
            
            VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(kerbalVesselId);
            VesselRemoveSystem.Singleton.AddToKillList(kerbalVesselId, "Killing kerbal as it boarded a external seat");
            LockSystem.Singleton.ReleaseAllVesselLocks(new[] { kerbalName }, kerbalVesselId);

            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(seat.vessel, true, false);
        }

        public void ExternalSeatUnboard(Vessel unboardedVessel, KerbalEVA kerbal)
        {
            if (unboardedVessel == null || kerbal.vessel == null) return;

            LunaLog.Log("Crew-unboard from an external seat detected!");
            
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(unboardedVessel, true, false);
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(kerbal.vessel, true, false);
        }
    }
}
