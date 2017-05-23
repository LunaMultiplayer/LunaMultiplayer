using System;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Types;
using UnityEngine;

namespace LunaClient.Systems.VesselChangeSys
{
    /// <summary>
    /// This system handle the changes of a vessel, part decouples and so on.
    /// </summary>
    public class VesselChangeSystem : MessageSystem<VesselChangeSystem, VesselChangeMessageSender, VesselChangeMessageHandler>
    {
        #region Fields & properties

        public Queue<VesselChangeMsgData> IncomingChanges { get; set; } = new Queue<VesselChangeMsgData>();
        public VesselChangeEvents VesselChangeEvents { get; } = new VesselChangeEvents();

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onStageSeparation.Add(VesselChangeEvents.OnStageSeparation);
            GameEvents.onPartDie.Add(VesselChangeEvents.OnPartDie);
            SetupRoutine(new RoutineDefinition(500, RoutineExecution.Update, ProcessVesselChanges));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            IncomingChanges.Clear();
            GameEvents.onStageSeparation.Remove(VesselChangeEvents.OnStageSeparation);
            GameEvents.onPartDie.Remove(VesselChangeEvents.OnPartDie);
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Picks changes received and process them
        /// </summary>
        private void ProcessVesselChanges()
        {
            if (Enabled && MainSystem.Singleton.GameRunning)
            {
                while (IncomingChanges.Count > 0 && Time.fixedTime - IncomingChanges.Peek().ReceiveTime >= 0.5)
                {
                    HandleVesselChange(IncomingChanges.Dequeue());
                }
            }
        }

        #endregion

        #region Private methods

        private static void HandleVesselChange(VesselChangeMsgData messageData)
        {
            var vessel = FlightGlobals.VesselsLoaded.FirstOrDefault(v => v.id == messageData.VesselId);
            if (vessel != null && !vessel.packed && (VesselCommon.IsSpectating || vessel.id != FlightGlobals.ActiveVessel.id))
            {
                switch ((VesselChangeType)messageData.ChangeType)
                {
                    case VesselChangeType.EXPLODE:
                        var part = vessel.Parts.FirstOrDefault(p => p.flightID == messageData.PartFlightId) ??
                                    vessel.Parts.FirstOrDefault(p => p.craftID == messageData.PartCraftId);

                        part?.explode();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion
    }
}
