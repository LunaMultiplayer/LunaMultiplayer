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

        private static int _updateNearbySendMSInterval = 500;
        private static int _updateSendMSInterval = 3000;
        private static string SEND_TIMER_NAME = "SEND";


        #endregion

        #region Base overrides

        public override void OnEnabled()
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
                         ? _updateNearbySendMSInterval
                         : _updateSendMSInterval);
            }
        }

        #endregion
    }
}
