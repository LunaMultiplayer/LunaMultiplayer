using UniLinq;

namespace LunaClient.Systems.VesselChangeSys
{
    public class VesselChange
    {
        public int Stage { get; set; } = int.MinValue;
        public uint[] PartsToRemove { get; set; }
        public uint[] ShieldsToClose { get; set; }
        public uint[] ShieldsToOpen { get; set; }
        public uint[] PartsToExtend { get; set; }
        public uint[] PartsToRetract { get; set; }

        public bool HasChanges()
        {
            return Stage > int.MinValue ||
                (PartsToRemove != null && PartsToRemove.Any()) ||
                (ShieldsToClose != null && ShieldsToClose.Any()) ||
                (ShieldsToOpen != null && ShieldsToOpen.Any()) ||
                (PartsToExtend != null && PartsToExtend.Any()) ||
                (PartsToRetract != null && PartsToRetract.Any());
        }
    }
}