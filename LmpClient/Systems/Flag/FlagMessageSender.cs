using System;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Flag;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.Flag
{
    public class FlagMessageSender : SubSystem<FlagSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<FlagCliMsg>(msg)));
        }

        public FlagDataMsgData GetFlagMessageData(string flagName, byte[] flagData)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<FlagDataMsgData>();
            msgData.Flag.Owner = SettingsSystem.CurrentSettings.PlayerName;
            msgData.Flag.FlagName = flagName;
            msgData.Flag.NumBytes = flagData.Length;
            
            if (msgData.Flag.FlagData.Length < flagData.Length)
                msgData.Flag.FlagData = new byte[flagData.Length];

            Array.Copy(flagData, msgData.Flag.FlagData, flagData.Length);

            return msgData;
        }
    }
}