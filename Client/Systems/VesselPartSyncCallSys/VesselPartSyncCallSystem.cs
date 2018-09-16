using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Systems.TimeSyncer;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient.Systems.VesselPartSyncCallSys
{
    /// <summary>
    /// This system sends the part module calls to the other players. 
    /// An example would be selecting "Activate engine" action when you right click on an engine and press that part action
    /// Another would be "Extend" in the retractable ladder part
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
            PartModuleEvent.onPartModuleMethodCalling.Add(VesselPartModuleSyncCallEvents.PartModuleMethodCalled);
            SetupRoutine(new RoutineDefinition(250, RoutineExecution.Update, ProcessVesselPartSyncCalls));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            PartModuleEvent.onPartModuleMethodCalling.Remove(VesselPartModuleSyncCallEvents.PartModuleMethodCalled);

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
