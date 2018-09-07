using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.ShareScienceSubject;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselUtilities;
using System;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoEvents : SubSystem<VesselProtoSystem>
    {
        /// <summary>
        /// Sends our vessel just when we start the flight
        /// </summary>
        public void FlightReady()
        {
            if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || FlightGlobals.ActiveVessel.id == Guid.Empty)
                return;

            if (!System.CheckVessel(FlightGlobals.ActiveVessel))
            {
                VesselRemoveSystem.Singleton.AddToKillList(FlightGlobals.ActiveVessel.id, "Vessel check not passed");
                VesselRemoveSystem.Singleton.KillVessel(FlightGlobals.ActiveVessel.id, "Vessel check not passed");
                return;
            }

            System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, false);
        }

        /// <summary>
        /// Called when a vessel is initiated.
        /// </summary>
        public void VesselCreate(Vessel data)
        {
            //The vessel is being created by the loader
            if (VesselLoader.CurrentlyLoadingVesselId == data.id)
                return;
            
            //This happens when vessel crashes
            if (VesselCommon.IsSpectating)
            {
                VesselRemoveSystem.Singleton.AddToKillList(data.id, "Tried to create a new vessel while spectating");
                return;
            }

            System.MessageSender.SendVesselMessage(data, false);
            LockSystem.Singleton.AcquireUpdateLock(data.id, true);
        }

        /// <summary>
        /// Event called when switching scene and before reaching the other scene
        /// </summary>
        internal void OnSceneRequested(GameScenes requestedScene)
        {
            if (HighLogic.LoadedSceneIsFlight && requestedScene != GameScenes.FLIGHT)
            {
                //When quitting flight send the vessel one last time
                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, false);
            }
        }

        /// <summary>
        /// Triggered when the vessel parts change. We use this to detect if we are spectating and our vessel is different than the controller. 
        /// If that's the case we trigger a reload
        /// </summary>
        public void VesselPartCountChangedInSpectatingVessel(Vessel vessel)
        {
            if (vessel == null) return;
            
            if (!VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || VesselCommon.IsSpectating && FlightGlobals.ActiveVessel.id != vessel.id) return;

            if (vessel.protoVessel.protoPartSnapshots.Count != FlightGlobals.ActiveVessel.Parts.Count)
                VesselLoader.LoadVessel(vessel.protoVessel);
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
                    System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, false);

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
                    //We need to FORCE the clients to reload this vessel. Otherwise they won't get an updated protomodule
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
                //We need to FORCE the clients to reload this vessel. Otherwise they won't get an updated protomodule
                System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);
            }
        }
    }
}
