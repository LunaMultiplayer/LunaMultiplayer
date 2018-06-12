using LunaCommon.Xml;
using System;

namespace Server.Settings.Definition
{
    [Serializable]
    public class InterpolationSettingsDefinition
    {
        [XmlComment(Value = "Force clients to have the interpolation offset specified below")]
        public bool ForceInterpolationOffset { get; set; } = false;

        [XmlComment(Value = "The interpolation offset in milliseconds to force the clients to have")]
        public int InterpolationOffsetMs { get; set; } = 1500;

        [XmlComment(Value = "Force clients to have interpolation on/off")]
        public bool ForceInterpolation { get; set; } = false;

        [XmlComment(Value = "The interpolation value to force the clients to have")]
        public bool InterpolationValue { get; set; } = true;

        [XmlComment(Value = "Force clients to have extrapolation on/off")]
        public bool ForceExtrapolation { get; set; } = false;

        [XmlComment(Value = "The extrapolation value to force the clients to have")]
        public bool ExtrapolationValue { get; set; } = true;
    }
}
