using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Systems.Mod;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselUtilities;
using LunaClient.Windows.BannedParts;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace LunaClient.Systems.VesselProtoSys
{
    /// <summary>
    /// This system handles the vessel loading into the game and sending our vessel structure to other players.
    /// </summary>
    public class VesselProtoSystem : MessageSystem<VesselProtoSystem, VesselProtoMessageSender, VesselProtoMessageHandler>
    {
        #region Fields & properties

        public ConcurrentDictionary<Guid, VesselProtoQueue> VesselProtos { get; } = new ConcurrentDictionary<Guid, VesselProtoQueue>();

        public static Guid CurrentlyUpdatingVesselId { get; set; } = Guid.Empty;

        public bool ProtoSystemReady => Enabled && FlightGlobals.ready && HighLogic.LoadedScene == GameScenes.FLIGHT && 
            FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating;

        public VesselProtoEvents VesselProtoEvents { get; } = new VesselProtoEvents();

        public VesselRemoveSystem VesselRemoveSystem => VesselRemoveSystem.Singleton;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselProtoSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            VesselInitializeEvent.onVesselInitialized.Add(VesselProtoEvents.VesselInitialized);
            GameEvents.onFlightReady.Add(VesselProtoEvents.FlightReady);
            GameEvents.onGameSceneLoadRequested.Add(VesselProtoEvents.OnSceneRequested);
            GameEvents.onVesselPartCountChanged.Add(VesselProtoEvents.VesselPartCountChanged);

            GameEvents.OnTriggeredDataTransmission.Add(VesselProtoEvents.TriggeredDataTransmission);
            GameEvents.OnExperimentStored.Add(VesselProtoEvents.ExperimentStored);
            ExperimentEvent.onExperimentReset.Add(VesselProtoEvents.ExperimentReset);

            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, CheckVesselsToLoad));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, SendVesselDefinition));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            VesselInitializeEvent.onVesselInitialized.Remove(VesselProtoEvents.VesselInitialized);
            GameEvents.onFlightReady.Remove(VesselProtoEvents.FlightReady);
            GameEvents.onGameSceneLoadRequested.Remove(VesselProtoEvents.OnSceneRequested);
            GameEvents.onVesselPartCountChanged.Remove(VesselProtoEvents.VesselPartCountChanged);

            GameEvents.OnTriggeredDataTransmission.Remove(VesselProtoEvents.TriggeredDataTransmission);
            GameEvents.OnExperimentStored.Remove(VesselProtoEvents.ExperimentStored);
            ExperimentEvent.onExperimentReset.Remove(VesselProtoEvents.ExperimentReset);

            //This is the main system that handles the vesselstore so if it's disabled clear the store aswell
            VesselProtos.Clear();
        }

        #endregion

        #region Public

        /// <summary>
        /// Checks the vessel for invalid parts
        /// </summary>
        public bool CheckVessel(Vessel vessel)
        {
            if (vessel == null || vessel.isEVA || vessel.vesselType == VesselType.Flag) return true;

            if (ModSystem.Singleton.ModControl)
            {
                var bannedParts = ModSystem.Singleton.GetBannedPartsFromVessel(vessel.protoVessel).ToArray();
                if (bannedParts.Any())
                {
                    LunaLog.LogError($"Vessel {vessel.id}-{vessel.vesselName} Contains the following banned parts: {string.Join(", ", bannedParts)}");
                    BannedPartsWindow.Singleton.DisplayBannedPartsDialog(vessel, bannedParts);
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Update routines

        /// <summary>
        /// Send the definition of our own vessel and the secondary vessels.
        /// </summary>
        private void SendVesselDefinition()
        {
            try
            {
                if (ProtoSystemReady)
                {
                    if (FlightGlobals.ActiveVessel.parts.Count != FlightGlobals.ActiveVessel.protoVessel.protoPartSnapshots.Count)
                        MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, false);

                    foreach (var vessel in VesselCommon.GetSecondaryVessels())
                    {
                        if (vessel.parts.Count != vessel.protoVessel.protoPartSnapshots.Count)
                            MessageSender.SendVesselMessage(vessel, false);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in SendVesselDefinition {e}");
            }

        }

        /// <summary>
        /// Check vessels that must be loaded
        /// </summary>
        private void CheckVesselsToLoad()
        {
            try
            {
                foreach (var keyVal in VesselProtos)
                {
                    if (keyVal.Value.TryPeek(out var vesselProto) && vesselProto.GameTime <= TimeSyncerSystem.UniversalTime)
                    {
                        keyVal.Value.TryDequeue(out _);

                        if (VesselRemoveSystem.VesselWillBeKilled(vesselProto.VesselId))
                            continue;

                        var protoVessel = vesselProto.CreateProtoVessel();

                        var existingVessel = FlightGlobals.FindVessel(vesselProto.VesselId);
                        if (existingVessel == null)
                        {
                            LunaLog.Log($"[LMP]: Loading vessel {vesselProto.VesselId}");
                            if (VesselLoader.LoadVessel(protoVessel))
                            {
                                LunaLog.Log($"[LMP]: Vessel {protoVessel.vesselID} loaded");
                                VesselLoadEvent.onLmpVesselLoaded.Fire(protoVessel.vesselRef);
                            }
                        }
                        else
                        {
                            LunaLog.Log($"[LMP]: Reloading vessel {vesselProto.VesselId}");
                            if (VesselLoader.LoadVessel(protoVessel))
                            {
                                LunaLog.Log($"[LMP]: Vessel {protoVessel.vesselID} reloaded");
                                VesselReloadEvent.onLmpVesselReloaded.Fire(protoVessel.vesselRef);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in CheckVesselsToLoad {e}");
            }
        }

        #endregion
    }
}
