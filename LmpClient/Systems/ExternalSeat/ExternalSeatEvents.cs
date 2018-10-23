using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Utilities;
using LmpClient.VesselUtilities;
using System;

namespace LmpClient.Systems.ExternalSeat
{
    public class ExternalSeatEvents : SubSystem<ExternalSeatSystem>
    {
        public void ExternalSeatBoard(Vessel vessel, Guid kerbalVesselId, string kerbalName)
        {
            //Do not check if we are spectating as we are perhaps boarding a seat of a vessel controlled by another player!
            if (vessel == null) return;

            LunaLog.Log("Crew-board to an external seat detected!");

            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel);

            VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(kerbalVesselId);
            VesselRemoveSystem.Singleton.AddToKillList(kerbalVesselId, "Killing kerbal as it boarded a external seat");
            LockSystem.Singleton.ReleaseAllVesselLocks(new[] { kerbalName }, kerbalVesselId);

            VesselCommon.RemoveVesselFromSystems(kerbalVesselId);
        }

        public void ExternalSeatUnboard(Vessel unboardedVessel, KerbalEVA kerbal)
        {
            if (VesselCommon.IsSpectating) return;
            if (unboardedVessel == null || kerbal.vessel == null) return;

            LunaLog.Log("Crew-unboard from an external seat detected!");
            
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(unboardedVessel);

            EvaReady.FireOnCrewEvaReady(kerbal);
        }

        /// <summary>
        /// Kerbal in eva is initialized with orbit data and ready to be sent to the server
        /// </summary>
        public void CrewEvaReady(Vessel evaVessel)
        {
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(evaVessel);
        }
    }
}
