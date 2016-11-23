using System.Collections;
using LunaClient.Base;
using UnityEngine;
using LunaClient.Systems.SettingsSys;

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
        
        private static float _updateSendSInterval = 0.5f;
        private static float _updateLowSendSInterval = 3f;

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(SendVesselUpdates());
        }

        #endregion
        
        #region Private methods
        
        /// <summary>
        /// Send control state, clamps, decouplers and dock ports of our vessel
        /// </summary>
        private IEnumerator SendVesselUpdates()
        {
            var seconds = new WaitForSeconds(_updateSendSInterval);
            var secondsFar = new WaitForSeconds(_updateLowSendSInterval);
            while (true)
            {
                if (!Enabled) break;

                if(UpdateSystemReady)
                    MessageSender.SendVesselUpdate();

                if(VesselCommon.PlayerVesselsNearby())
                    yield return seconds;
                else
                    yield return secondsFar;
            }
        }

        #endregion
    }
}
