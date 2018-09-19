using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.VesselUtilities;
using System;

namespace LmpClient.Systems.ExternalSeat
{
    public class ExternalSeatEvents : SubSystem<ExternalSeatSystem>
    {
        public void ExternalSeatBoard(KerbalSeat seat, Guid kerbalVesselId, uint kerbalVesselPersistentId, string kerbalName)
        {
            if (VesselCommon.IsSpectating) return;

            if (seat.vessel == null) return;

            LunaLog.Log("Crew-board to an external seat detected!");
            
            VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(kerbalVesselId, kerbalVesselPersistentId);
            VesselRemoveSystem.Singleton.AddToKillList(kerbalVesselPersistentId, kerbalVesselId, "Killing kerbal as it boarded a external seat");
            LockSystem.Singleton.ReleaseAllVesselLocks(new[] { kerbalName }, kerbalVesselId, kerbalVesselPersistentId);

            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(seat.vessel, false);
        }

        public void ExternalSeatUnboard(Vessel unboardedVessel, KerbalEVA kerbal)
        {
            if (VesselCommon.IsSpectating) return;

            if (unboardedVessel == null || kerbal.vessel == null) return;

            LunaLog.Log("Crew-unboard from an external seat detected!");
            
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(unboardedVessel, false);
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(kerbal.vessel, false);
        }
    }
}
