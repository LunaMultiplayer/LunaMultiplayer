using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselRemoveSys;
using System;

namespace LunaClient.Systems.ExternalSeat
{
    public class ExternalSeatEvents : SubSystem<ExternalSeatSystem>
    {
        public void ExternalSeatBoard(KerbalSeat seat, Guid kerbalVesselId, string kerbalName)
        {
            if (seat.vessel == null) return;

            LunaLog.Log("Crewboard to an external seat detected!");
            
            VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(kerbalVesselId, false);
            VesselRemoveSystem.Singleton.AddToKillList(kerbalVesselId, "Killing kerbal as it boarded a vessel");
            LockSystem.Singleton.ReleaseAllVesselLocks(new[] { kerbalName }, kerbalVesselId);

            //This vessel will have a new kerbal as a part so we can be sure that the other clients need a reload of the vessel
            VesselProtoSys.VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(seat.vessel, true, true);
        }

        public void ExternalSeatUnboard(KerbalSeat seat, KerbalEVA kerbal)
        {
            if (seat.vessel == null || kerbal.vessel == null) return;

            LunaLog.Log("Crewunboard from an external seat detected!");
            
            //This vessel will have kerbal removed as a part so we can be sure that the other clients need a reload of the vessel
            VesselProtoSys.VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(seat.vessel, true, true);

            VesselProtoSys.VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(kerbal.vessel, true, false);
        }
    }
}
