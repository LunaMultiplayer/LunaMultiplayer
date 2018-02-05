using LunaClient.Base;
using LunaClient.VesselUtilities;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselUpdateSystem : MessageSystem<VesselUpdateSystem, VesselUpdateMessageSender, VesselUpdateMessageHandler>
    {
        #region Fields & properties

        public bool UpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
                                                 FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                                 FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
                                                 FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        private List<Vessel> SecondaryVesselsToUpdate { get; } = new List<Vessel>();
        private List<Vessel> AbandonedVesselsToUpdate { get; } = new List<Vessel>();

        #endregion

        #region Base overrides
        
        public override string SystemName { get; } = nameof(VesselUpdateSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, SendVesselUpdates));
            SetupRoutine(new RoutineDefinition(5000, RoutineExecution.Update, SendSecondaryVesselUpdates));
            SetupRoutine(new RoutineDefinition(10000, RoutineExecution.Update, SendUnloadedSecondaryVesselUpdates));
        }
        #endregion

        #region Update routines

        private void SendVesselUpdates()
        {
            if (UpdateSystemReady && !VesselCommon.IsSpectating)
            {
                MessageSender.SendVesselUpdate(FlightGlobals.ActiveVessel);
            }
        }

        private void SendSecondaryVesselUpdates()
        {
            if (UpdateSystemReady && !VesselCommon.IsSpectating)
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
            if (UpdateSystemReady && !VesselCommon.IsSpectating)
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
