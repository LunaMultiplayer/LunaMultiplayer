using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Warp
{
    public class WarpMessageSender : SubSystem<WarpSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<WarpCliMsg>(msg)));
        }

        /// <summary>
        /// Sends the EXISTING subspace that we jumped into
        /// </summary>
        public void SendChangeSubspaceMsg(int subspaceId)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<WarpChangeSubspaceMsgData>();
            msgData.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
            msgData.Subspace = subspaceId;

            SendMessage(msgData);
        }

        /// <summary>
        /// Sends the new subspace that we jumped into
        /// </summary>
        public void SendNewSubspace()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<WarpNewSubspaceMsgData>();
            msgData.ServerTimeDifference = TimeSyncerSystem.UniversalTime - TimeSyncerSystem.ServerClockSec;
            msgData.PlayerCreator = SettingsSystem.CurrentSettings.PlayerName;
            //we don't send the SubspaceKey as it will be given by the server except when warping that we set it to -1

            SendMessage(msgData);
        }
    }
}