using LunaClient.Base;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using UnityEngine;

namespace LunaClient.Systems.VesselRemoveSys
{
    public class VesselRemoveEvents : SubSystem<VesselRemoveSystem>
    {
        /// <summary>
        /// This event is called when the vessel gone BOOM
        /// If we have the update lock of it we kill it
        /// It doesn't matter if we own the control lock or not as perhaps we are killing a vessel of a player who disconnected.
        /// </summary>
        public void OnVesselDestroyed(Vessel dyingVessel)
        {
            if (dyingVessel.state != Vessel.State.DEAD)
                return;

            //Only remove the vessel if we own the update lock
            if (SystemsContainer.Get<LockSystem>().LockIsOurs($"update-{dyingVessel.id}"))
            {
                Debug.Log($"[LMP]: Removing vessel {dyingVessel.id}, Name: {dyingVessel.vesselName} from the server: Destroyed");
                SystemsContainer.Get<KerbalSystem>().MessageSender.SendKerbalsInVessel(dyingVessel);

                System.MessageSender.SendVesselRemove(dyingVessel.id);

                //Vessel is dead so remove the locks
                SystemsContainer.Get<LockSystem>().ReleaseLock($"control-{dyingVessel.id}");
                SystemsContainer.Get<LockSystem>().ReleaseLock($"update-{dyingVessel.id}");
            }
        }

        /// <summary>
        /// This event is called when the vessel is recovered
        /// </summary>
        public void OnVesselRecovered(ProtoVessel recoveredVessel, bool quick)
        {
            //quick == true when you press "space center" from the inflight menu

            if (!VesselControlLockIsOurs(recoveredVessel))
            {
                ScreenMessages.PostScreenMessage("Cannot recover vessel, the vessel is not yours.", 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            Debug.Log($"[LMP]: Removing vessel {recoveredVessel.vesselID}, Name: {recoveredVessel.vesselName} from the server: Recovered");
            SystemsContainer.Get<KerbalSystem>().MessageSender.SendKerbalsInVessel(recoveredVessel);

            System.MessageSender.SendVesselRemove(recoveredVessel.vesselID);

            //Vessel is recovered so remove the locks
            SystemsContainer.Get<LockSystem>().ReleaseLock($"control-{recoveredVessel.vesselID}");
            SystemsContainer.Get<LockSystem>().ReleaseLock($"update-{recoveredVessel.vesselID}");
        }

        /// <summary>
        /// This event is called when vessel is terminated from track station
        /// </summary>
        public void OnVesselTerminated(ProtoVessel terminatedVessel)
        {
            if (!VesselControlLockIsOurs(terminatedVessel))
            {
                ScreenMessages.PostScreenMessage("Cannot terminate vessel, the vessel is not yours.", 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            Debug.Log($"[LMP]: Removing vessel {terminatedVessel.vesselID}, Name: {terminatedVessel.vesselName} from the server: Terminated");
            SystemsContainer.Get<KerbalSystem>().MessageSender.SendKerbalsInVessel(terminatedVessel);

            System.MessageSender.SendVesselRemove(terminatedVessel.vesselID);

            //Vessel is terminated so remove locks
            SystemsContainer.Get<LockSystem>().ReleaseLock($"control-{terminatedVessel.vesselID}");
            SystemsContainer.Get<LockSystem>().ReleaseLock($"update-{terminatedVessel.vesselID}");
        }

        /// <summary>
        /// Check if a vessel is yours or not
        /// </summary>
        /// <returns></returns>
        private static bool VesselControlLockIsOurs(ProtoVessel vessel)
        {
            return !SystemsContainer.Get<LockSystem>().LockExists($"control-{vessel.vesselID}") || SystemsContainer.Get<LockSystem>().LockIsOurs($"control-{vessel.vesselID}");
        }
    }
}