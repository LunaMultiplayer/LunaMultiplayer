using System.Collections;
using LunaClient.Base;
using UnityEngine;
using System.Diagnostics;

namespace LunaClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// System that handle the received vessel update messages and also sends them
    /// </summary>
    public class VesselUpdateSystem : MessageSystem<VesselUpdateSystem, VesselUpdateMessageSender, VesselUpdateMessageHandler>
    {
        #region Constructors
        public VesselUpdateSystem() : base()
        {
            setupTimer(SEND_TIMER_NAME, _updateSendMSInterval);
        }
        #endregion

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
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Send control state, clamps, decouplers and dock ports of our vessel
        /// </summary>
        public override void FixedUpdate()
        {
            if (!Enabled || !UpdateSystemReady)
            {
                return;
            }

            if (IsTimeForNextSend(SEND_TIMER_NAME))
            {
                Profiler.BeginSample("VesselUpdateSystem");
                MessageSender.SendVesselUpdate();
                Profiler.EndSample();
            }
        }

        private void setMsElapsedForNextSend()
        {
            if(VesselCommon.PlayerVesselsNearby())
            {
                setupTimer(SEND_TIMER_NAME, _updateNearbySendMSInterval);
            } else
            {
                setupTimer(SEND_TIMER_NAME, _updateSendMSInterval);
            }
        }

        #endregion
    }
}
