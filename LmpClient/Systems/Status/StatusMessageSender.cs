using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.PlayerStatus;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.Status
{
    public class StatusMessageSender : SubSystem<StatusSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<PlayerStatusCliMsg>(msg)));
        }

        public void SendPlayersRequest()
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<PlayerStatusCliMsg, PlayerStatusRequestMsgData>()));
        }

        public void SendOwnStatus()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<PlayerStatusSetMsgData>();
            msgData.PlayerStatus.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
            msgData.PlayerStatus.StatusText = System.MyPlayerStatus.StatusText;
            msgData.PlayerStatus.VesselText = System.MyPlayerStatus.VesselText;

            SendMessage(msgData);
        }
    }
}