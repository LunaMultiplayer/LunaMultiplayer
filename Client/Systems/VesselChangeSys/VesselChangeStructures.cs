using UniLinq;

namespace LunaClient.Systems.VesselChangeSys
{
    public class VesselChange
    {
        public int Stage { get; set; } = int.MinValue;

        /// <summary>
        /// Currently unused as when vessel part count is different we reload the whole vessel
        /// </summary>
        public uint[] PartsToRemove { get; set; }

        public uint[] ShieldsToClose { get; set; }
        public uint[] ShieldsToOpen { get; set; }
        public uint[] PartsToExtend { get; set; }
        public uint[] PartsToRetract { get; set; }
        public uint[] LaddersToExtend { get; set; }
        public uint[] LaddersToRetract { get; set; }
        public uint[] EnginesToStart { get; set; }
        public uint[] EnginesToStop { get; set; }
        public uint[] DockingPortsToDisable { get; set; }
        public uint[] DockingPortsToEnable { get; set; }

        public KSPActionGroup[] ActionGroupsToEnable { get; set; }
        public KSPActionGroup[] ActionGroupsToDisable { get; set; }

        public bool HasChanges()
        {
            return Stage > int.MinValue ||
                (PartsToRemove != null && PartsToRemove.Any()) ||
                (ShieldsToClose != null && ShieldsToClose.Any()) ||
                (ShieldsToOpen != null && ShieldsToOpen.Any()) ||
                (PartsToExtend != null && PartsToExtend.Any()) ||
                (PartsToRetract != null && PartsToRetract.Any()) ||
                (LaddersToExtend != null && LaddersToExtend.Any()) ||
                (LaddersToRetract != null && LaddersToRetract.Any()) ||
                (EnginesToStart != null && EnginesToStart.Any()) ||
                (EnginesToStop != null && EnginesToStop.Any()) ||
                (DockingPortsToDisable != null && DockingPortsToDisable.Any()) ||
                (DockingPortsToEnable != null && DockingPortsToEnable.Any()) ||
                (ActionGroupsToEnable != null && ActionGroupsToEnable.Any()) ||
                (ActionGroupsToDisable != null && ActionGroupsToDisable.Any());
        }
    }
}