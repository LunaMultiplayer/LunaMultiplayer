using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Status
{
    public class StatusMessageSender : SubSystem<StatusSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<PlayerStatusCliMsg>(msg)));
        }

        public void SendPlayerStatus(PlayerStatus playerStatus)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<PlayerStatusSetMsgData>();
            msgData.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
            msgData.StatusText = playerStatus.StatusText;
            msgData.VesselText = playerStatus.VesselText;

            SendMessage(msgData);
        }

        public void SendOwnStatus()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<PlayerStatusSetMsgData>();
            msgData.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
            msgData.StatusText = System.MyPlayerStatus.StatusText;
            msgData.VesselText = System.MyPlayerStatus.VesselText;

            SendMessage(msgData);
        }
    }
}