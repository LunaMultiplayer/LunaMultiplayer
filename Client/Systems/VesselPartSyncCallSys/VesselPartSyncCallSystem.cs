using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Systems.TimeSyncer;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient.Systems.VesselPartSyncCallSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselPartSyncCallSystem : MessageSystem<VesselPartSyncCallSystem, VesselPartSyncCallMessageSender, VesselPartSyncCallMessageHandler>
    {
        #region Fields & properties

        public bool PartSyncSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT && Time.timeSinceLevelLoad > 1f;

        private VesselPartSyncCallEvents VesselPartModuleSyncCallEvents { get; } = new VesselPartSyncCallEvents();

        public ConcurrentDictionary<Guid, VesselPartSyncCallQueue> VesselPartsSyncs { get; } = new ConcurrentDictionary<Guid, VesselPartSyncCallQueue>();

        #endregion

        #region Base overrides        

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselPartSyncCallSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            PartModuleEvent.onPartModuleMethodCalled.Add(VesselPartModuleSyncCallEvents.PartModuleMethodCalled);
            SetupRoutine(new RoutineDefinition(250, RoutineExecution.Update, ProcessVesselPartSyncCalls));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            PartModuleEvent.onPartModuleMethodCalled.Remove(VesselPartModuleSyncCallEvents.PartModuleMethodCalled);

            VesselPartsSyncs.Clear();
        }

        #endregion

        #region Update routines

        private void ProcessVesselPartSyncCalls()
        {
            foreach (var keyVal in VesselPartsSyncs)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncerSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessPartMethodCallSync();
                    keyVal.Value.Recycle(update);
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Removes a vessel from the system
        /// </summary>
        public void RemoveVessel(Guid vesselId)
        {
            VesselPartsSyncs.TryRemove(vesselId, out _);
        }

        #endregion
    }
}
