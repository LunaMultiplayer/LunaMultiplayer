using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.VesselLockSys;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Types;
using UnityEngine;

namespace LunaClient.Systems.VesselChangeSys
{
    /// <summary>
    /// This system handle the changes of a vessel, part decouples and so on.
    /// </summary>
    public class VesselChangeSystem :
        MessageSystem<VesselChangeSystem, VesselChangeMessageSender, VesselChangeMessageHandler>
    {
        public Queue<VesselChangeMsgData> IncomingChanges { get; set; } = new Queue<VesselChangeMsgData>();
        public VesselChangeEvents VesselChangeEvents { get; } = new VesselChangeEvents();

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onStageSeparation.Add(VesselChangeEvents.OnStageSeparation);
            GameEvents.onPartDie.Add(VesselChangeEvents.OnPartDie);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            IncomingChanges.Clear();
            GameEvents.onStageSeparation.Remove(VesselChangeEvents.OnStageSeparation);
            GameEvents.onPartDie.Remove(VesselChangeEvents.OnPartDie);
        }

        /// <summary>
        /// Picks changes received and process them
        /// </summary>
        public override void Update()
        {
            base.Update();
            if (Enabled && MainSystem.Singleton.GameRunning && Timer.ElapsedMilliseconds > 500)
            {
                while (IncomingChanges.Count > 0 && Time.fixedTime - IncomingChanges.Peek().ReceiveTime >= 0.5)
                {
                    HandleVesselChange(IncomingChanges.Dequeue());
                }
            }
        }

        #endregion
        
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
    }
}
