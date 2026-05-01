using System;
using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.TimeSync;

namespace LmpClient.Systems.VesselActionGroupSys
{
    /// <summary>
    /// This class sends and processes the action groups
    /// </summary>
    public class VesselActionGroupSystem : MessageSystem<VesselActionGroupSystem, VesselActionGroupMessageSender, VesselActionGroupMessageHandler>
    {
        #region Fields & properties

        public ConcurrentDictionary<Guid, VesselActionGroupQueue> VesselActionGroups { get; } = new ConcurrentDictionary<Guid, VesselActionGroupQueue>();

        public static VesselActionGroupEvents VesselActionGroupEvents { get; } = new VesselActionGroupEvents();

        #endregion

        #region Base overrides

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselActionGroupSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();

            // Attempt to load all the from-future messages we've been storing
            TimingManager.UpdateAdd(HandlePositionsStage, MessageHandler.OnUpdate);

            SetupRoutine(new RoutineDefinition(500, RoutineExecution.Update, ProcessVesselActionGroups));

            // Debug logging
            SetupRoutine(new RoutineDefinition(10000, RoutineExecution.Update, MessageHandler.LogQueuedMessagesSize));

            ActionGroupEvent.onActionGroupFired.Add(VesselActionGroupEvents.ActionGroupFired);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            VesselActionGroups.Clear();

            TimingManager.UpdateRemove(HandlePositionsStage, MessageHandler.OnUpdate);

            ActionGroupEvent.onActionGroupFired.Remove(VesselActionGroupEvents.ActionGroupFired);
        }

        #endregion

        #region Update routines

        private void ProcessVesselActionGroups()
        {
            foreach (var keyVal in VesselActionGroups)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessActionGroup();
                    keyVal.Value.Recycle(update);
                }
            }
        }

        #endregion
    }
}
