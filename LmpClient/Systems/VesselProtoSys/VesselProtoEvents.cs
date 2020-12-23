using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.ShareScienceSubject;
using LmpClient.VesselUtilities;
using System;

namespace LmpClient.Systems.VesselProtoSys
{
    public class VesselProtoEvents : SubSystem<VesselProtoSystem>
    {
        /// <summary>
        /// When stop warping, spawn the missing vessels
        /// </summary>
        public void WarpStopped()
        {
            System.CheckVesselsToLoad();
        }

        /// <summary>
        /// Sends our vessel just when we start the flight
        /// </summary>
        public void FlightReady()
        {
            if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || FlightGlobals.ActiveVessel.id == Guid.Empty)
                return;

            System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);
        }

        /// <summary>
        /// Event called when switching scene and before reaching the other scene
        /// </summary>
        internal void OnSceneRequested(GameScenes requestedScene)
        {
            if (HighLogic.LoadedSceneIsFlight && requestedScene != GameScenes.FLIGHT && !VesselCommon.IsSpectating)
            {
                //When quitting flight send the vessel one last time
                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel);
            }
        }

        /// <summary>
        /// Triggered when transmitting science. Science experiment is stored in the vessel so send the definition to the server
        /// </summary>
        public void TriggeredDataTransmission(ScienceData science, Vessel vessel, bool data)
        {
            if (FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating)
            {
                //We must send the science subject aswell!
                var subject = ResearchAndDevelopment.GetSubjectByID(science.subjectID);
                if (subject != null)
                {
                    LunaLog.Log("Detected a experiment transmission. Sending vessel definition to the server");
                    System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);

                    ShareScienceSubjectSystem.Singleton.MessageSender.SendScienceSubjectMessage(subject);
                }
            }
        }

        /// <summary>
        /// Triggered when storing science. Science experiment is stored in the vessel so send the definition to the server
        /// </summary>
        public void ExperimentStored(ScienceData science)
        {
            if (FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating)
            {
                //We must send the science subject aswell!
                var subject = ResearchAndDevelopment.GetSubjectByID(science.subjectID);
                if (subject != null)
                {
                    LunaLog.Log("Detected a experiment stored. Sending vessel definition to the server");
                    System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);

                    ShareScienceSubjectSystem.Singleton.MessageSender.SendScienceSubjectMessage(subject);
                }
            }
        }

        /// <summary>
        /// Triggered when resetting a experiment. Science experiment is stored in the vessel so send the definition to the server
        /// </summary>
        public void ExperimentReset(Vessel data)
        {
            if (FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating)
            {
                LunaLog.Log("Detected a experiment reset. Sending vessel definition to the server");
                System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);
            }
        }

        public void PartUndocked(Part part, DockedVesselInfo dockedInfo, Vessel originalVessel)
        {
            if (VesselCommon.IsSpectating) return;
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(originalVessel.id, SettingsSystem.CurrentSettings.PlayerName)) return;

            System.MessageSender.SendVesselMessage(part.vessel);

            //As this method can be called several times in a short period (when staging) we delay the sending of the final vessel
            System.DelayedSendVesselMessage(originalVessel.id, 0.5f);
        }

        public void PartDecoupled(Part part, float breakForce, Vessel originalVessel)
        {
            if (VesselCommon.IsSpectating || originalVessel == null) return;
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(originalVessel.id, SettingsSystem.CurrentSettings.PlayerName)) return;

            System.MessageSender.SendVesselMessage(part.vessel);

            //As this method can be called several times in a short period (when staging) we delay the sending of the final vessel
            System.DelayedSendVesselMessage(originalVessel.id, 0.5f);
        }

        public void PartCoupled(Part partFrom, Part partTo, Guid removedVesselId)
        {
            if (VesselCommon.IsSpectating) return;

            //If neither the vessel 1 or vessel2 locks belong to us, ignore the coupling
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(partFrom.vessel.id, SettingsSystem.CurrentSettings.PlayerName) &&
                !LockSystem.LockQuery.UpdateLockBelongsToPlayer(removedVesselId, SettingsSystem.CurrentSettings.PlayerName)) return;

            System.MessageSender.SendVesselMessage(partFrom.vessel);
        }

        public void EVAConstructionModePartAttached(Vessel vessel, Part part)
        {
            if (VesselCommon.IsSpectating) return;

            System.MessageSender.SendVesselMessage(vessel);
        }

        public void EVAConstructionModePartDetached(Vessel vessel, Part part)
        {
            if (VesselCommon.IsSpectating) return;

            System.MessageSender.SendVesselMessage(vessel);
        }
    }
}
