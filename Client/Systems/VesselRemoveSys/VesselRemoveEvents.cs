using LunaClient.Base;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;

namespace LunaClient.Systems.VesselRemoveSys
{
    public class VesselRemoveEvents : SubSystem<VesselRemoveSystem>
    {
        /// <summary>
        /// This event is called when the vessel gone BOOM (the Vessel.Die() is called)
        /// If we have the update lock of it we kill it
        /// It doesn't matter if we own the control lock or not as perhaps we are killing a vessel of a player who disconnected.
        /// </summary>
        public void OnVesselWillDestroy(Vessel dyingVessel)
        {
            //Only remove the vessel if we own the update lock
            if (LockSystem.LockQuery.UpdateLockBelongsToPlayer(dyingVessel.id, SettingsSystem.CurrentSettings.PlayerName))
            {
                LunaLog.Log($"[LMP]: Removing vessel {dyingVessel.id}, Name: {dyingVessel.vesselName} from the server: Destroyed");
                SystemsContainer.Get<KerbalSystem>().ProcessKerbalsInVessel(dyingVessel);

                System.MessageSender.SendVesselRemove(dyingVessel.id);

                //Vessel is dead so remove the locks
                SystemsContainer.Get<LockSystem>().ReleaseAllVesselLocks(dyingVessel.id);
            }
        }

        /// <summary>
        /// This event is called when the vessel is recovered
        /// </summary>
        public void OnVesselRecovered(ProtoVessel recoveredVessel, bool quick)
        {
            //quick == true when you press "space center" from the inflight menu

            if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(recoveredVessel.vesselID, SettingsSystem.CurrentSettings.PlayerName))
            {
                ScreenMessages.PostScreenMessage("Cannot recover vessel, the vessel is not yours.", 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            LunaLog.Log($"[LMP]: Removing vessel {recoveredVessel.vesselID}, Name: {recoveredVessel.vesselName} from the server: Recovered");
            SystemsContainer.Get<KerbalSystem>().ProcessKerbalsInVessel(recoveredVessel);

            System.MessageSender.SendVesselRemove(recoveredVessel.vesselID);

            //Vessel is recovered so remove the locks
            SystemsContainer.Get<LockSystem>().ReleaseAllVesselLocks(recoveredVessel.vesselID);
        }

        /// <summary>
        /// This event is called when vessel is terminated from track station
        /// </summary>
        public void OnVesselTerminated(ProtoVessel terminatedVessel)
        {
            if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(terminatedVessel.vesselID, SettingsSystem.CurrentSettings.PlayerName))
            {
                ScreenMessages.PostScreenMessage("Cannot terminate vessel, the vessel is not yours.", 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            LunaLog.Log($"[LMP]: Removing vessel {terminatedVessel.vesselID}, Name: {terminatedVessel.vesselName} from the server: Terminated");
            SystemsContainer.Get<KerbalSystem>().ProcessKerbalsInVessel(terminatedVessel);

            System.MessageSender.SendVesselRemove(terminatedVessel.vesselID);

            //Vessel is terminated so remove locks            
            SystemsContainer.Get<LockSystem>().ReleaseAllVesselLocks(terminatedVessel.vesselID);
        }
    }
}