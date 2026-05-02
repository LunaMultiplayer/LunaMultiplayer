using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.TimeSync;
using System;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselDecoupleSys
{
    /// <summary>
    /// This class syncs the decouples between players
    /// </summary>
    public class VesselDecoupleSystem : MessageSystem<VesselDecoupleSystem, VesselDecoupleMessageSender, VesselDecoupleMessageHandler>
    {
        #region Fields & properties

        public ConcurrentDictionary<Guid, VesselDecoupleQueue> VesselDecouples { get; } = new ConcurrentDictionary<Guid, VesselDecoupleQueue>();
        private VesselDecoupleEvents VesselDecoupleEvents { get; } = new VesselDecoupleEvents();
        public bool IgnoreEvents { get; set; }
        public Guid ManuallyDecouplingVesselId { get; set; }

        #endregion

        #region Base overrides        

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselDecoupleSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            PartEvent.onPartDecoupling.Add(VesselDecoupleEvents.DecoupleStart);
            PartEvent.onPartDecoupled.Add(VesselDecoupleEvents.DecoupleComplete);

            // Attempt to load all the from-future messages we've been storing
            TimingManager.UpdateAdd(TimingManager.TimingStage.BetterLateThanNever, MessageHandler.OnUpdate);

            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, ProcessVesselDecouples));
            
            // Debug logging
            SetupRoutine(new RoutineDefinition(10000, RoutineExecution.Update, MessageHandler.LogQueuedMessagesSize));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            PartEvent.onPartDecoupling.Remove(VesselDecoupleEvents.DecoupleStart);
            PartEvent.onPartDecoupled.Remove(VesselDecoupleEvents.DecoupleComplete);

            TimingManager.UpdateRemove(TimingManager.TimingStage.BetterLateThanNever, MessageHandler.OnUpdate);

            VesselDecouples.Clear();
        }

        #endregion

        #region Update routines

        private void ProcessVesselDecouples()
        {
            foreach (var keyVal in VesselDecouples)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessDecouple();
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
            VesselDecouples.TryRemove(vesselId, out _);
        }

        #endregion
    }
}
