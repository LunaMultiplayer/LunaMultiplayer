using System;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.SettingsSys
{
    public class SettingsMessageSender : SubSystem<SettingsSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            throw new NotImplementedException("We never send settings!");
        }
    }
}