using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.ColorSystem
{
    public class PlayerColorMessageSender : SubSystem<PlayerColorSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<PlayerColorCliMsg>(msg));
        }

        public void SendPlayerColorToServer()
        {
            var data = new PlayerColorSetMsgData
            {
                PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                Color = System.ConvertColorToString(SettingsSystem.CurrentSettings.PlayerColor)
            };
            SendMessage(data);
        }
    }
}