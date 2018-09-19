using LmpClient.Base;
using UnityEngine;

namespace LmpClient.Systems.VesselSyncSys
{
    /// <summary>
    /// This system syncs your current vessels that you have in proto store against the server vessels. Server will send you the vessels that you don't have if necessary.
    /// </summary>
    public class VesselSyncSystem : MessageSystem<VesselSyncSystem, VesselSyncMessageSender, VesselSyncMessageHandler>
    {
        #region Fields & properties

        public bool UpdateSystemReady => Enabled && Time.timeSinceLevelLoad > 1f;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselSyncSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(10000, RoutineExecution.Update, SendCurrentVesselIds));
        }

        #endregion

        #region Update routines

        private void SendCurrentVesselIds()
        {
            if (UpdateSystemReady)
            {
                MessageSender.SendVesselsSyncMsg();
            }
        }

        #endregion
    }
}
