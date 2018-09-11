using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Systems.TimeSyncer;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient.Systems.VesselPartSyncSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselPartSyncSystem : MessageSystem<VesselPartSyncSystem, VesselPartSyncMessageSender, VesselPartSyncMessageHandler>
    {
        #region Fields & properties

        public bool PartSyncSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT && Time.timeSinceLevelLoad > 1f;

        private VesselPartSyncEvents VesselPartModuleSyncEvents { get; } = new VesselPartSyncEvents();

        public ConcurrentDictionary<Guid, VesselPartSyncQueue> VesselPartsSyncs { get; } = new ConcurrentDictionary<Guid, VesselPartSyncQueue>();

        #endregion

        #region Base overrides        

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselPartSyncSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            PartModuleEvent.onPartModuleActionCalled.Add(VesselPartModuleSyncEvents.PartModuleActionCalled);
            PartModuleEvent.onPartModuleEventCalled.Add(VesselPartModuleSyncEvents.PartModuleEventCalled);
            PartModuleEvent.onPartModuleMethodCalled.Add(VesselPartModuleSyncEvents.PartModuleMethodCalled);
            SetupRoutine(new RoutineDefinition(250, RoutineExecution.Update, ProcessVesselPartSyncs));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            PartModuleEvent.onPartModuleActionCalled.Remove(VesselPartModuleSyncEvents.PartModuleActionCalled);
            PartModuleEvent.onPartModuleEventCalled.Remove(VesselPartModuleSyncEvents.PartModuleEventCalled);
            PartModuleEvent.onPartModuleMethodCalled.Remove(VesselPartModuleSyncEvents.PartModuleMethodCalled);
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
                    update.ProcessPartMethodSync();
                    keyVal.Value.Recycle(update);
                }
            }
        }

        #endregion
    }
}
