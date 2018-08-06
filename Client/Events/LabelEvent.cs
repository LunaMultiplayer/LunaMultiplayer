using KSP.UI.Screens;
using KSP.UI.Screens.Mapview;
using LunaClient.Events.Base;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public class LabelEvent : LmpBaseEvent
    {
        public static EventData<BaseLabel> onLabelProcessed;
        public static EventData<Vessel, MapNode.CaptionData> onMapLabelProcessed;
        public static EventData<TrackingStationWidget> onMapWidgetTextProcessed;
    }
}
