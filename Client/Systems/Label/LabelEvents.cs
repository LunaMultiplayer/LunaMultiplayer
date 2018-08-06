using KSP.UI.Screens;
using KSP.UI.Screens.Mapview;
using LunaClient.Base;
using LunaClient.Systems.Lock;

namespace LunaClient.Systems.Label
{
    public class LabelEvents : SubSystem<LabelSystem>
    {
        public void OnLabelProcessed(BaseLabel label)
        {
            if (label is VesselLabel vesselLabel)
            {
                var vessel = vesselLabel.vessel;
                var owner = LockSystem.LockQuery.GetControlLockOwner(vessel.id);

                if (!string.IsNullOrEmpty(owner))
                    label.text.text = $"{owner}\n{label.text.text}";
            }
        }

        public void OnMapLabelProcessed(Vessel vessel, MapNode.CaptionData label)
        {
            if (vessel == null) return;

            var owner = LockSystem.LockQuery.GetControlLockOwner(vessel.id);
            if (!string.IsNullOrEmpty(owner))
            {
                label.Header = $"{owner}\n{label.Header}";
            }
        }

        public void OnMapWidgetTextProcessed(TrackingStationWidget widget)
        {
            if (widget.vessel == null) return;

            var owner = LockSystem.LockQuery.GetControlLockOwner(widget.vessel.id);
            if (!string.IsNullOrEmpty(owner))
            {
                widget.textName.text = $"({owner}) {widget.textName.text}";
            }
        }
    }
}
