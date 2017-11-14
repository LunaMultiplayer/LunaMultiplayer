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
        public uint[] EnginesToStart { get; set; }
        public uint[] EnginesToStop { get; set; }
        public KSPActionGroup[] ActionGroupsToToggle { get; set; }

        public bool HasChanges()
        {
            return Stage > int.MinValue ||
                (PartsToRemove != null && PartsToRemove.Any()) ||
                (ShieldsToClose != null && ShieldsToClose.Any()) ||
                (ShieldsToOpen != null && ShieldsToOpen.Any()) ||
                (PartsToExtend != null && PartsToExtend.Any()) ||
                (PartsToRetract != null && PartsToRetract.Any()) ||
                (EnginesToStart != null && EnginesToStart.Any()) ||
                (EnginesToStop != null && EnginesToStop.Any()) ||
                (ActionGroupsToToggle != null && ActionGroupsToToggle.Any());
        }
    }
}