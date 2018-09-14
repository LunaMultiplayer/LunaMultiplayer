using LunaClient.Base;
using LunaClient.Systems.TimeSyncer;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselFairingsSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselFairingsSystem : MessageSystem<VesselFairingsSystem, VesselFairingsMessageSender, VesselFairingsMessageHandler>
    {
        #region Fields & properties
        
        public ConcurrentDictionary<Guid, VesselFairingQueue> VesselFairings { get; } = new ConcurrentDictionary<Guid, VesselFairingQueue>();
        private VesselFairingEvents VesselFairingEvents { get; } = new VesselFairingEvents();

        #endregion

        #region Base overrides        

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselFairingsSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onFairingsDeployed.Add(VesselFairingEvents.FairingsDeployed);
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, ProcessVesselFairings));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            VesselFairings.Clear();
        }

        #endregion

        #region Update routines

        private void ProcessVesselFairings()
        {
            foreach (var keyVal in VesselFairings)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncerSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessFairing();
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
            VesselFairings.TryRemove(vesselId, out _);
        }
        
        #endregion
    }
}
