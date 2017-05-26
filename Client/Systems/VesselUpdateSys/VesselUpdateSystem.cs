using LunaClient.Base;
using UnityEngine;

namespace LunaClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// System that handle the received vessel update messages and also sends them
    /// </summary>
    public class VesselUpdateSystem : MessageSystem<VesselUpdateSystem, VesselUpdateMessageSender, VesselUpdateMessageHandler>
    {
        #region Field & Properties

        public bool UpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
                                         FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded && !VesselCommon.IsSpectating &&
                                         FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
                                         FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public FlightCtrlState FlightState { get; set; }

        private const int UpdateNearbySendMsInterval = 500;
        private const int UpdateSendMsInterval = 3000;

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(3000, RoutineExecution.Update, SendVesselUpdates));
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Send control state, clamps, decouplers and dock ports of our vessel
        /// </summary>
        private void SendVesselUpdates()
        {
            if (Enabled && UpdateSystemReady)
            {
                MessageSender.SendVesselUpdate();
                ChangeRoutineExecutionInterval("SendVesselUpdates",
                     VesselCommon.PlayerVesselsNearby()
                         ? UpdateNearbySendMsInterval
                         : UpdateSendMsInterval);
            }
        }

        #endregion
    }
}
