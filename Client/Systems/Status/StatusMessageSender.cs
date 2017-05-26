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
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<PlayerStatusCliMsg>(msg));
        }

        public void SendPlayerStatus(PlayerStatus playerStatus)
        {
            SendMessage(new PlayerStatusSetMsgData
            {
                PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                StatusText = playerStatus.StatusText,
                VesselText = playerStatus.VesselText
            });
        }
    }
}