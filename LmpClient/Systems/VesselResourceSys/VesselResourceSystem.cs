using LmpClient.Base;
using LmpClient.Systems.TimeSyncer;
using LmpClient.VesselUtilities;
using System;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselResourceSys
{
    public class VesselResourceSystem : MessageSystem<VesselResourceSystem, VesselResourceMessageSender, VesselResourceMessageHandler>
    {
        #region Fields & properties

        public ConcurrentDictionary<Guid, VesselResourceQueue> VesselResources { get; } = new ConcurrentDictionary<Guid, VesselResourceQueue>();

        #endregion

        #region Base overrides        

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselResourceSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(2500, RoutineExecution.Update, SendVesselResources));
            SetupRoutine(new RoutineDefinition(2500, RoutineExecution.Update, ProcessVesselResources));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            VesselResources.Clear();
        }

        #endregion

        #region Update routines

        private void ProcessVesselResources()
        {
            if (HighLogic.LoadedScene < GameScenes.SPACECENTER) return;

            foreach (var keyVal in VesselResources)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncerSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessVesselResource();
                    keyVal.Value.Recycle(update);
                }
            }
        }

        private void SendVesselResources()
        {
            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.loaded && !VesselCommon.IsSpectating)
            {
                MessageSender.SendVesselResources(FlightGlobals.ActiveVessel);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Removes a vessel from the system
        /// </summary>
        public void RemoveVessel(Guid vesselId)
        {
            VesselResources.TryRemove(vesselId, out _);
        }

        #endregion
    }
}
