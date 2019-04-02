using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Color;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.PlayerColorSys
{
    public class PlayerColorMessageSender : SubSystem<PlayerColorSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<PlayerColorCliMsg>(msg)));
        }

        public void SendColorsRequest()
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<PlayerColorCliMsg, PlayerColorRequestMsgData>()));
        }

        public void SendPlayerColorToServer()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<PlayerColorSetMsgData>();
            msgData.PlayerColor.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
            msgData.PlayerColor.Color = SettingsSystem.CurrentSettings.PlayerColor;

            SendMessage(msgData);
        }
    }
}