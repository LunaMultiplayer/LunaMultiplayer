using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.TimeSync;
using System;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselUndockSys
{
    /// <summary>
    /// This class syncs the undocks between players
    /// </summary>
    public class VesselUndockSystem : MessageSystem<VesselUndockSystem, VesselUndockMessageSender, VesselUndockMessageHandler>
    {
        #region Fields & properties
        
        public ConcurrentDictionary<Guid, VesselUndockQueue> VesselUndocks { get; } = new ConcurrentDictionary<Guid, VesselUndockQueue>();
        private VesselUndockEvents VesselUndockEvents { get; } = new VesselUndockEvents();
        public bool IgnoreEvents { get; set; }
        public Guid ManuallyUndockingVesselId { get; set; }

        #endregion

        #region Base overrides        

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselUndockSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            PartEvent.onPartUndocking.Add(VesselUndockEvents.UndockStart);
            PartEvent.onPartUndocked.Add(VesselUndockEvents.UndockComplete);

            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, ProcessVesselUndocks));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            PartEvent.onPartUndocking.Remove(VesselUndockEvents.UndockStart);
            PartEvent.onPartUndocked.Remove(VesselUndockEvents.UndockComplete);

            VesselUndocks.Clear();
        }

        #endregion

        #region Update routines

        private void ProcessVesselUndocks()
        {
            foreach (var keyVal in VesselUndocks)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessUndock();
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
            VesselUndocks.TryRemove(vesselId, out _);
        }
        
        #endregion
    }
}
