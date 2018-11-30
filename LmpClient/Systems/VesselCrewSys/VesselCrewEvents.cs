using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Utilities;
using System;

namespace LmpClient.Systems.VesselCrewSys
{
    public class VesselCrewEvents : SubSystem<VesselCrewSystem>
    {
        /// <summary>
        /// Event triggered when a kerbal boards a vessel
        /// </summary>
        public void OnCrewBoard(Guid kerbalId, string kerbalName, Vessel vessel)
        {
            LunaLog.Log("Crew boarding detected!");

            VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(kerbalId, false);
            LockSystem.Singleton.ReleaseAllVesselLocks(new[] { kerbalName }, kerbalId);
            VesselRemoveSystem.Singleton.KillVessel(kerbalId, true, "Killing kerbal-vessel as it boarded a vessel");

            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel, true);
        }
        
        /// <summary>
        /// Trigger an event once the kerbal in EVA is ready to be sent
        /// </summary>
        public void OnCrewEva(GameEvents.FromToAction<Part, Part> data)
        {
            EvaReady.FireOnCrewEvaReady(data.to.FindModuleImplementing<KerbalEVA>());
        }

        /// <summary>
        /// Kerbal in eva is initialized with orbit data and ready to be sent to the server
        /// </summary>
        public void CrewEvaReady(Vessel evaVessel)
        {
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(evaVessel, true);
        }

        /// <summary>
        /// Crew in the vessel has been modified so send the vessel to the server
        /// </summary>
        public void OnCrewModified(Vessel vessel)
        {
            if(!vessel.isEVA && LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel, true);
        }
    }
}
