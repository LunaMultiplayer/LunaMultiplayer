using LunaClient.Base;

namespace LunaClient.Systems.VesselPersistentSys
{
    /// <summary>
    /// This class sends some parts of the vessel information to other players. We do it in another system as we don't want to send this information so often as
    /// the vessel position system and also we want to send it more oftenly than the vessel proto.
    /// </summary>
    public class VesselPersistentSystem : MessageSystem<VesselPersistentSystem, VesselPersistentMessageSender, VesselPersistentMessageHandler>
    {
        #region Fields & properties
        
        private VesselPersistentEvents VesselPersistentEvents { get; } = new VesselPersistentEvents();

        #endregion

        #region Base overrides        
        
        public override string SystemName { get; } = nameof(VesselPersistentSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onPartPersistentIdChanged.Add(VesselPersistentEvents.PartPersistentIdChanged);
            GameEvents.onVesselPersistentIdChanged.Add(VesselPersistentEvents.VesselPersistentIdChanged);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onPartPersistentIdChanged.Remove(VesselPersistentEvents.PartPersistentIdChanged);
            GameEvents.onVesselPersistentIdChanged.Remove(VesselPersistentEvents.VesselPersistentIdChanged);
        }

        #endregion
    }
}
