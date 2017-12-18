using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Interface;
using System.IO;

namespace LunaClient.Systems.Flag
{
    public class FlagMessageSender : SubSystem<FlagSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<FlagCliMsg>(msg)));
        }

        public FlagDataMsgData GetFlagMessageData(string flagName, string fullFlagPath)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<FlagDataMsgData>();
            if (msgData.Flag != null)
            {
                msgData.Flag.Owner = SettingsSystem.CurrentSettings.PlayerName;
                msgData.Flag.FlagName = flagName;
                msgData.Flag.FlagData = File.ReadAllBytes(fullFlagPath);
            }
            else
            {
                //Try to avoid the creation of new classes to not generate garbage...
                msgData.Flag = new FlagInfo
                {
                    Owner = SettingsSystem.CurrentSettings.PlayerName,
                    FlagName = flagName,
                    FlagData = File.ReadAllBytes(fullFlagPath)
                };
            }
            return msgData;
        }
    }
}