using LunaClient.Base;
using LunaClient.Events;
using UnityEngine;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselPartModuleSyncSystem : MessageSystem<VesselPartModuleSyncSystem, VesselPartModuleSyncMessageSender, VesselPartModuleSyncMessageHandler>
    {
        #region Fields & properties

        public bool PartSyncSystemReady => Enabled && HighLogic.LoadedScene >= GameScenes.FLIGHT && Time.timeSinceLevelLoad > 1f;

        private VesselPartModuleSyncEvents VesselPartModuleSyncEvents { get; } = new VesselPartModuleSyncEvents();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselPartModuleSyncSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            PartModuleEvent.onPartModuleFieldChange.Add(VesselPartModuleSyncEvents.PartModuleFieldChange);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            PartModuleEvent.onPartModuleFieldChange.Remove(VesselPartModuleSyncEvents.PartModuleFieldChange);
        }

        #endregion
    }
}
