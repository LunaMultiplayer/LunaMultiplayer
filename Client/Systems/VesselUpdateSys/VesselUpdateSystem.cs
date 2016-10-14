using System;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using UnityEngine;

namespace LunaClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// System that handle the received vessel update messages and also sends them
    /// </summary>
    public class VesselUpdateSystem : MessageSystem<VesselUpdateSystem, VesselUpdateMessageSender, VesselUpdateMessageHandler>
    {
        #region Field & Properties

        private bool _enabled;
        public override bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled && !value)
                {
                    InterpolationSystem.ResetSystem();
                    ReceivedUpdates.Clear();
                }

                _enabled = value;
            }
        }

        public bool UpdateSystemReady
        {
            get
            {
                if (!Enabled || FlightGlobals.ActiveVessel == null || !HighLogic.LoadedSceneIsFlight || !FlightGlobals.ready ||
                    Time.timeSinceLevelLoad < 1f || !FlightGlobals.ActiveVessel.loaded ||
                    FlightGlobals.ActiveVessel.state == Vessel.State.DEAD ||
                    FlightGlobals.ActiveVessel.packed ||
                    FlightGlobals.ActiveVessel.vesselType == VesselType.Flag ||
                    VesselCommon.ActiveVesselIsInSafetyBubble())
                    return false;
                return true;
            }
        }
        
        public Dictionary<Guid, Queue<VesselUpdate>> ReceivedUpdates { get; } = new Dictionary<Guid, Queue<VesselUpdate>>();

        private double LastSendTime { get; set; }

        private VesselUpdateInterpolationSystem InterpolationSystem { get; } = new VesselUpdateInterpolationSystem();

        #endregion

        #region Base overrides

        public override void Update()
        {
            base.Update();
            if (!UpdateSystemReady)
                return;

            SendVesselUpdates();
            InterpolationSystem.HandleVesselUpdates();
            InterpolationSystem.RemoveVessels();
        }

        #endregion

        #region Public methods

        public bool VesselHasUpdates(Guid vesselId, int minNumberOfUpdates)
        {
            return ReceivedUpdates.ContainsKey(vesselId) && ReceivedUpdates[vesselId].Count >= minNumberOfUpdates;
        }

        public int GetNumberOfUpdatesInQueue()
        {
            return ReceivedUpdates.Sum(u => u.Value.Count);
        }

        public double GetMsInPast()
        {
            return VesselUpdateInterpolationSystem.MsInPast;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Send the updates of our own vessel and the secondary vessels. We only send them after an interval specified
        /// </summary>
        private void SendVesselUpdates()
        {
            if (DateTime.Now.Ticks - LastSendTime >= TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselUpdatesSendMsInterval).Ticks)
            {
                LastSendTime = DateTime.Now.Ticks;
                SendVesselUpdate(FlightGlobals.ActiveVessel);
                SendSecondaryVesselUpdates();
            }
        }

        /// <summary>
        /// Send updates for vessels that we own the update lock. 
        /// Check UpdateUpdateLocks() in VesselMainSystem to see how we get this locks
        /// </summary>
        private void SendSecondaryVesselUpdates()
        {
            var secondaryVesselsIdsToUpdate = LockSystem.Singleton.GetLocks(SettingsSystem.CurrentSettings.PlayerName)
                .Where(l => l.StartsWith("update-"))
                .Select(l => l.Substring(7))
                .Where(i => i != FlightGlobals.ActiveVessel.id.ToString())
                .ToArray();

            foreach (var secondryVessel in secondaryVesselsIdsToUpdate)
            {
                var vessel = FlightGlobals.Vessels.SingleOrDefault(v => v.id.ToString() == secondryVessel);
                if (vessel != null)
                    SendVesselUpdate(vessel);
            }
        }

        /// <summary>
        /// Create and send the vessel update
        /// </summary>
        /// <param name="checkVessel"></param>
        private void SendVesselUpdate(Vessel checkVessel)
        {
            var update = VesselUpdate.CreateFromVessel(checkVessel);
            if (update != null)
            {
                MessageSender.SendVesselUpdate(update);
            }
            else
            {
                Debug.LogError("Cannot send vessel update!");
            }
        }

        #endregion
    }
}
