using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Settings;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.SettingsSys
{
    public class SettingsMessageSender : SubSystem<SettingsSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            throw new NotImplementedException("We never send settings in this way!");
        }

        public void SendSettingsRequest()
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<SettingsCliMsg, SettingsRequestMsgData>()));
        }
    }
}