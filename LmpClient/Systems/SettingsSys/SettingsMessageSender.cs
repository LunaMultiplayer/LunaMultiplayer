using System;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.SettingsSys
{
    public class SettingsMessageSender : SubSystem<SettingsSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            throw new NotImplementedException("We never send settings!");
        }
    }
}