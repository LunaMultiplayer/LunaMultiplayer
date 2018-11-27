using LmpClient.Systems.VesselPositionSys;
using System;

namespace LmpClient.Windows.Vessels.Structures
{
    internal class VesselInterpolationDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }
        public int Amount { get; set; }
        public float Percentage { get; set; }
        public double Duration { get; set; }
        public double ExtraInterpolationTime { get; set; }
        public double TimeDifference { get; set; }

        public VesselInterpolationDisplay(Guid vesselId) => VesselId = vesselId;

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;

            Amount = 0;
            Percentage = 0;
            Duration = 0;
            ExtraInterpolationTime = 0;
            TimeDifference = 0;

            if (VesselPositionSystem.CurrentVesselUpdate.TryGetValue(VesselId, out var current) && current.Target != null)
            {
                if (VesselPositionSystem.TargetVesselUpdateQueue.TryGetValue(VesselId, out var queue))
                {
                    Amount = queue.Count;
                }
                Percentage = current.LerpPercentage * 100;
                Duration = TimeSpan.FromSeconds(current.InterpolationDuration).TotalMilliseconds;
                ExtraInterpolationTime = TimeSpan.FromSeconds(current.ExtraInterpolationTime).TotalMilliseconds;
                TimeDifference = TimeSpan.FromSeconds(current.TimeDifference).TotalMilliseconds;
            }
        }

        protected override void PrintDisplay()
        {
            StringBuilder.Length = 0;
            StringBuilder.Append("Amt: ").AppendLine(Amount.ToString());
            StringBuilder.Append("Duration: ").AppendLine($"{Duration:F0}ms");
            StringBuilder.Append("TimeDiff: ").AppendLine($"{TimeDifference:F0}ms");
            StringBuilder.Append("ExtraInterpolationTime: ").AppendLine($"+{ExtraInterpolationTime:F0}ms");
            StringBuilder.Append("Percentage: ").AppendLine($"{Percentage:F0}%");
        }
    }
}
