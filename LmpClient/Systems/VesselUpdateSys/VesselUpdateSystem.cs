using LmpClient.Base;
using LmpClient.Systems.TimeSyncer;
using LmpClient.VesselUtilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LmpClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselUpdateSystem : MessageSystem<VesselUpdateSystem, VesselUpdateMessageSender, VesselUpdateMessageHandler>
    {
        #region Fields & properties

        private List<Vessel> SecondaryVesselsToUpdate { get; } = new List<Vessel>();
        private List<Vessel> AbandonedVesselsToUpdate { get; } = new List<Vessel>();

        public ConcurrentDictionary<Guid, VesselUpdateQueue> VesselUpdates { get; } = new ConcurrentDictionary<Guid, VesselUpdateQueue>();

        #endregion

        #region Base overrides

        protected override bool ProcessMessagesInUnityThread => false;

        public override string SystemName { get; } = nameof(VesselUpdateSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, SendVesselUpdates));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, ProcessVesselUpdates));
            SetupRoutine(new RoutineDefinition(5000, RoutineExecution.Update, SendSecondaryVesselUpdates));
            SetupRoutine(new RoutineDefinition(10000, RoutineExecution.Update, SendUnloadedSecondaryVesselUpdates));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            VesselUpdates.Clear();
        }

        #endregion

        #region Update routines

        private void ProcessVesselUpdates()
        {
            if (HighLogic.LoadedScene < GameScenes.SPACECENTER) return;

            foreach (var keyVal in VesselUpdates)
            {
                while (keyVal.Value.TryPeek(out var update) && update.GameTime <= TimeSyncerSystem.UniversalTime)
                {
                    keyVal.Value.TryDequeue(out update);
                    update.ProcessVesselUpdate();
                    keyVal.Value.Recycle(update);
                }
            }
        }

        private void SendVesselUpdates()
        {
            if (!VesselCommon.IsSpectating)
            {
                MessageSender.SendVesselUpdate(FlightGlobals.ActiveVessel);
            }
        }

        private void SendSecondaryVesselUpdates()
        {
            if (!VesselCommon.IsSpectating)
            {
                SecondaryVesselsToUpdate.Clear();
                SecondaryVesselsToUpdate.AddRange(VesselCommon.GetSecondaryVessels());

                for (var i = 0; i < SecondaryVesselsToUpdate.Count; i++)
                {
                    MessageSender.SendVesselUpdate(SecondaryVesselsToUpdate[i]);
                }
            }
        }

        private void SendUnloadedSecondaryVesselUpdates()
        {
            if (!VesselCommon.IsSpectating)
            {
                AbandonedVesselsToUpdate.Clear();
                AbandonedVesselsToUpdate.AddRange(VesselCommon.GetUnloadedSecondaryVessels());

                for (var i = 0; i < AbandonedVesselsToUpdate.Count; i++)
                {
                    MessageSender.SendVesselUpdate(AbandonedVesselsToUpdate[i]);
                }
            }
        }

        #endregion
    }
}
