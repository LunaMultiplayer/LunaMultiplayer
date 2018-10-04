using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.VesselUtilities;

namespace LmpClient.Systems.VesselCrewSys
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

                LunaLog.Log($"EVA Boarding. Kerbal: {kerbalVessel.vesselName} boarding: {vessel.vesselName}");
                if (LockSystem.LockQuery.ControlLockBelongsToPlayer(kerbalVessel.id, SettingsSystem.CurrentSettings.PlayerName))
                {
                    VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(kerbalVessel);
                    LockSystem.Singleton.ReleaseAllVesselLocks(new[] { kerbalVessel.vesselName }, kerbalVessel.id);
                }
                VesselRemoveSystem.Singleton.AddToKillList(kerbalVessel, "Killing kerbal as it boarded a vessel");

                //The vessel definition has changed so send the new vessel even if it's controlled by someone else
                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel);
            }
        }

        /// <summary>
        /// The vessel has changed as it has less crew now so send the definition
        /// </summary>
        public void OnCrewTransfered(GameEvents.HostedFromToAction<ProtoCrewMember, Part> data)
        {
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(data.from.vessel);
        }

        /// <summary>
        /// The vessel has changed as it has less crew now so send the definition.
        /// </summary>
        public void OnCrewEva(GameEvents.FromToAction<Part, Part> data)
        {
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(data.from.vessel);
            //Do not send the kerbal as his orbit is not ready. It will be handled by the VesselLockEvents.OnVesselChange
        }
    }
}
