using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.TimeSync;
using System;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselCoupleSys
{
    /// <summary>
    /// This class syncs the couples between players
    /// </summary>
    public class VesselCoupleSystem : MessageSystem<VesselCoupleSystem, VesselCoupleMessageSender, VesselCoupleMessageHandler>
    {
        #region Fields & properties

        public ConcurrentDictionary<Guid, VesselCoupleQueue> VesselCouples { get; } = new ConcurrentDictionary<Guid, VesselCoupleQueue>();
        private VesselCoupleEvents VesselCoupleEvents { get; } = new VesselCoupleEvents();
        public bool IgnoreEvents { get; set; }

        #endregion

        #region Base overrides        

        /// <inheritdoc />
        /// <summary>
        /// This is one of the few (probably the only) queued systems that must be run in the unity thread
        /// The reason is that when we receive a couple msg that affects OUR active vessel we must warp to the future
        /// </summary>
        protected override bool ProcessMessagesInUnityThread => true;

        public override string SystemName { get; } = nameof(VesselCoupleSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            PartEvent.onPartCoupling.Add(VesselCoupleEvents.CoupleStart);
            PartEvent.onPartCoupled.Add(VesselCoupleEvents.CoupleComplete);
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, ProcessVesselCouples));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            PartEvent.onPartCoupling.Remove(VesselCoupleEvents.CoupleStart);
            PartEvent.onPartCoupled.Remove(VesselCoupleEvents.CoupleComplete);
            VesselCouples.Clear();
        }

        #endregion

        #region Update routines

        private void ProcessVesselCouples()
        {
            foreach (var keyVal in VesselCouples)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessCouple();
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
            VesselCouples.TryRemove(vesselId, out _);
        }

        #endregion
    }
}
