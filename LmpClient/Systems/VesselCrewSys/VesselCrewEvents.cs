using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.Lock;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselRemoveSys;
using System;
using System.Collections;

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
            VesselRemoveSystem.Singleton.AddToKillList(kerbalId, "Killing kerbal as it boarded a vessel");

            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel);
        }
        
        /// <summary>
        /// Trigger an event once the kerbal in EVA is ready to be sent
        /// </summary>
        public void OnCrewEva(GameEvents.FromToAction<Part, Part> data)
        {
            MainSystem.Singleton.StartCoroutine(OnCrewEvaReady(data.to.FindModuleImplementing<KerbalEVA>()));

            IEnumerator OnCrewEvaReady(KerbalEVA eva)
            {
                while (eva != null && !eva.Ready)
                {
                    yield return null;
                }
                if (eva != null && eva.Ready)
                {
                    EvaEvent.onCrewEvaReady.Fire(eva.vessel);
                }
            }
        }

        /// <summary>
        /// Kerbal in eva is initialized with orbit data and ready to be sent to the server
        /// </summary>
        /// <param name="evaVessel"></param>
        public void CrewEvaReady(Vessel evaVessel)
        {
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(evaVessel);
        }

        /// <summary>
        /// Crew in the vessel has been modified so send the vessel to the server
        /// </summary>
        public void OnCrewModified(Vessel vessel)
        {
            if(!vessel.isEVA)
                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel);
        }
    }
}
