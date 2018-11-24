using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.TimeSync;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LmpClient.Systems.VesselPartSyncUiFieldSys
{
    /// <summary>
    /// This class sends the changes in the UI fields of a part module. An example of a UI field would be the "thrust percentage" of an engine
    /// </summary>
    public class VesselPartSyncUiFieldSystem : MessageSystem<VesselPartSyncUiFieldSystem, VesselPartSyncUiFieldMessageSender, VesselPartSyncUiFieldMessageHandler>
    {
        #region Fields & properties

        public bool PartSyncSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT && Time.timeSinceLevelLoad > 1f;

        private VesselPartSyncUiFieldEvents VesselPartModuleSyncUiFieldEvents { get; } = new VesselPartSyncUiFieldEvents();

        public ConcurrentDictionary<Guid, VesselPartSyncUiFieldQueue> VesselPartsUiFieldsSyncs { get; } = new ConcurrentDictionary<Guid, VesselPartSyncUiFieldQueue>();

        #endregion

        #region Base overrides        

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselPartSyncUiFieldSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();

            LockEvent.onLockAcquire.Add(VesselPartModuleSyncUiFieldEvents.LockAcquire);

            SetupRoutine(new RoutineDefinition(250, RoutineExecution.Update, ProcessVesselPartUiFieldsSyncs));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            LockEvent.onLockAcquire.Add(VesselPartModuleSyncUiFieldEvents.LockAcquire);

            VesselPartsUiFieldsSyncs.Clear();
        }

        #endregion

        #region Update routines

        private void ProcessVesselPartUiFieldsSyncs()
        {
            if (HighLogic.LoadedScene < GameScenes.SPACECENTER) return;

            foreach (var keyVal in VesselPartsUiFieldsSyncs)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessPartMethodSync();
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
            VesselPartsUiFieldsSyncs.TryRemove(vesselId, out _);
        }

        #endregion
    }
}
