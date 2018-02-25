using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselStateSys
{
    /// <summary>
    /// This class handles the states of the vessels (if they are packed or not) so they can be checked in another threads
    /// </summary>
    public class VesselStateSystem : Base.System<VesselStateSystem>
    {
        #region Fields & properties
        
        private VesselStateEvents VesselStateEvents { get; } = new VesselStateEvents();

        public static ConcurrentDictionary<Guid, Vessel> VesselsOnRails { get; } = new ConcurrentDictionary<Guid, Vessel>();
        public static ConcurrentDictionary<Guid, Vessel> VesselsOffRails { get; } = new ConcurrentDictionary<Guid, Vessel>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselStateSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselGoOnRails.Add(VesselStateEvents.OnVesselOnRails);
            GameEvents.onVesselGoOffRails.Add(VesselStateEvents.OnVesselOffRails);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ClearSystem();
            GameEvents.onVesselGoOnRails.Remove(VesselStateEvents.OnVesselOnRails);
            GameEvents.onVesselGoOffRails.Remove(VesselStateEvents.OnVesselOffRails);
        }

        #endregion

        public void ClearSystem()
        {
            VesselsOnRails.Clear();
            VesselsOffRails.Clear();
        }
    }
}
