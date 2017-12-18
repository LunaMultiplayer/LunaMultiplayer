using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Settings
{
    public class SettingsRequestMsgData : SettingsBaseMsgData
    {
        /// <inheritdoc />
        internal SettingsRequestMsgData() { }
        public override SettingsMessageType SettingsMessageType => SettingsMessageType.Request;

        public override string ClassName { get; } = nameof(SettingsRequestMsgData);
    }
}