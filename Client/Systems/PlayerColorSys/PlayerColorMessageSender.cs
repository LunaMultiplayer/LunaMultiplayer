using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.PlayerColorSys
{
    public class PlayerColorMessageSender : SubSystem<PlayerColorSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<PlayerColorCliMsg>(msg)));
        }

        public void SendPlayerColorToServer()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<PlayerColorSetMsgData>();
            msgData.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
            msgData.Color = System.ConvertColorToString(SettingsSystem.CurrentSettings.PlayerColor);
            
            SendMessage(msgData);
        }
    }
}