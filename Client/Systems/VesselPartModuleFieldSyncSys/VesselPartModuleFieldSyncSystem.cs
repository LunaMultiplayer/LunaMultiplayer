using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Systems.TimeSyncer;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient.Systems.VesselPartModuleFieldSyncSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselPartModuleFieldSyncSystem : MessageSystem<VesselPartModuleFieldSyncSystem, VesselPartModuleFieldSyncMessageSender, VesselPartModuleFieldSyncMessageHandler>
    {
        #region Fields & properties

        public bool PartSyncSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT && Time.timeSinceLevelLoad > 1f;

        private VesselPartModuleFieldSyncEvents VesselPartModuleSyncEvents { get; } = new VesselPartModuleFieldSyncEvents();

        public ConcurrentDictionary<Guid, VesselPartFieldSyncQueue> VesselPartsSyncs { get; } = new ConcurrentDictionary<Guid, VesselPartFieldSyncQueue>();

        #endregion

        #region Base overrides        

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselPartModuleFieldSyncSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            PartModuleEvent.onPartModuleFieldChange.Add(VesselPartModuleSyncEvents.PartModuleFieldChange);
            SetupRoutine(new RoutineDefinition(250, RoutineExecution.Update, ProcessVesselPartSyncs));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            PartModuleEvent.onPartModuleFieldChange.Remove(VesselPartModuleSyncEvents.PartModuleFieldChange);
        }

        #endregion

        #region Update routines

        private void ProcessVesselPartSyncs()
        {
            foreach (var keyVal in VesselPartsSyncs)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncerSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessPartSync();
                    keyVal.Value.Recycle(update);
                }
            }
        }

        #endregion
    }
}
