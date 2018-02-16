using LunaClient.Base;
using UnityEngine;

namespace LunaClient.Systems.VesselSyncSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselSyncSystem : MessageSystem<VesselSyncSystem, VesselSyncMessageSender, VesselSyncMessageHandler>
    {
        #region Fields & properties

        public bool UpdateSystemReady => Enabled && Time.timeSinceLevelLoad > 1f && HighLogic.LoadedScene >= GameScenes.FLIGHT;

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
