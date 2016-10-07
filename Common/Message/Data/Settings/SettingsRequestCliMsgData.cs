using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Settings
{
    public class SettingsRequestMsgData : SettingsBaseMsgData
    {
        public override SettingsMessageType SettingsMessageType => SettingsMessageType.REQUEST;
        //Empty message
    }
}