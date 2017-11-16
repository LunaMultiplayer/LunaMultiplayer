using LunaClient.Base;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Systems.VesselChangeSys
{
    /// This system applies vessel changes to the other vessels and send our changes. 
    /// A change is an antena that is extended or a solar pannel, or an engine that is ON/OFF, etc...
    /// We use this since reloading the whole vessel causes flickering and is slow.
    /// Bear in mind that this system and vessel proto system are very related. Therefore when we have a change in our vessel, we just 
    /// make a backup of it trough the vesselProtoSystem and send the vesselproto.
    /// Also we receive changes of other vessels as VesselProto so we must read trough them and apply the changes as needed
    public class VesselChangeSystem : Base.System
    {
        #region Fields & properties

        public ConcurrentDictionary<Guid, VesselChange> AllPlayerVesselChanges { get; } =
            new ConcurrentDictionary<Guid, VesselChange>();
        
        public bool ChangeSystemReady => Enabled && Time.timeSinceLevelLoad > 1f && FlightGlobals.ready &&
            HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating
            && FlightGlobals.ActiveVessel.state != Vessel.State.DEAD;

        public bool ChangeSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ready && FlightGlobals.ActiveVessel != null ||
            HighLogic.LoadedScene == GameScenes.TRACKSTATION;

        public VesselProtoEvents VesselProtoEvents { get; } = new VesselProtoEvents();

        public VesselRemoveSystem VesselRemoveSystem => SystemsContainer.Get<VesselRemoveSystem>();
        public VesselChangeChecker VesselChangeChecker { get; } = new VesselChangeChecker();
        private static List<Guid> ChangesProcessed { get; } = new List<Guid>();

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselWasModified.Add(VesselProtoEvents.VesselModified);
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, ProcessVesselChanges));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, CheckAndSendProtoIfVesselHasChanges));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselWasModified.Remove(VesselProtoEvents.VesselModified);
            AllPlayerVesselChanges.Clear();
            ChangesProcessed.Clear();
        }

        #endregion

        /// <summary>
        /// Clear the system stores. Call this when switching screens
        /// </summary>
        public void ClearSystem()
        {
            AllPlayerVesselChanges.Clear();
            ChangesProcessed.Clear();
        }

        /// <summary>
        /// When you receive a protovessel, call this method, then it will generate a change structure in the dictionary if there
        /// are changes to apply
        /// </summary>
        public void ProcessVesselChange(ProtoVessel newProtoVessel)
        {
            if (VesselsProtoStore.AllPlayerVessels.TryGetValue(newProtoVessel.vesselID, out var oldProtoUpdate))
            {
                var changes = VesselChangeDetector.GetProtoVesselChanges(oldProtoUpdate.ProtoVessel, newProtoVessel);
                if (changes != null && changes.HasChanges())
                {
                    AllPlayerVesselChanges.AddOrUpdate(newProtoVessel.vesselID, changes, (key, existingVal) => changes);
                }
            }
        }

        #region Update methods

        /// <summary>
        /// Here we run trough all the changes of the dictionary and apply them to the vessel.
        /// We extend/retract antennas, open/close shields, etc... Once done we just clear the dictionary
        /// </summary>
        private void ProcessVesselChanges()
        {
            try
            {
                if (ChangeSystemBasicReady)
                {
                    ChangesProcessed.Clear();
                    foreach (var vesselChange in AllPlayerVesselChanges)
                    {
                        var vessel = FlightGlobals.FindVessel(vesselChange.Key);
                        if (vessel == null) continue;

                        //The changes in a vessel (solar pannel that extends for example) involve touching the parts of a vessel.
                        //If a vessel is NOT loaded it's part list will be empty so there's no point in aplying changes to it
                        //we will apply them when the vessel is loaded later on...
                        if (!vessel.loaded)
                            continue;

                        VesselChangeApplier.ProcessVesselChanges(vessel, vesselChange.Value);
                        ChangesProcessed.Add(vesselChange.Key);
                    }

                    foreach (var key in ChangesProcessed)
                    {
                        AllPlayerVesselChanges.TryRemove(key, out _);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in ProcessVesselChanges {e}");
            }
        }

        /// <summary>
        /// Here we check if our OWN vessel has changes like panels extended and things like that.
        /// If so we call the protosystem to send an update of our vessel definition
        /// </summary>
        private void CheckAndSendProtoIfVesselHasChanges()
        {
            try
            {
                if (ChangeSystemReady)
                {
                    if(VesselChangeChecker.CheckActiveVesselHasChanges())
                        SystemsContainer.Get<VesselProtoSystem>().MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel);
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in CheckAndSendProtoIfVesselHasChanges {e}");
            }
        }

        #endregion
    }
}
