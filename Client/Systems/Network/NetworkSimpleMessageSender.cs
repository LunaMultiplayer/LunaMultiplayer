using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Data.Warp;

namespace LunaClient.Systems.Network
{
    public class NetworkSimpleMessageSender : SubSystem<NetworkSystem>
    {
        public void SendMotdRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<MotdCliMsg>(new MotdRequestMsgData()));
        }

        public void SendKerbalsRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<KerbalCliMsg>(new KerbalsRequestMsgData()));
        }

        public void SendVesselListRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(new VesselListRequestMsgData()));
        }

        public void SendSettingsRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<SettingsCliMsg>(new SettingsRequestMsgData()));
        }

        public void SendWarpSubspacesRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<WarpCliMsg>(new WarpSubspacesRequestMsgData()));
        }

        public void SendColorsRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<PlayerColorCliMsg>(new PlayerColorRequestMsgData()));
        }

        public void SendPlayersRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<PlayerStatusCliMsg>(new PlayerStatusRequestMsgData()));
        }

        public void SendScenariosRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<ScenarioCliMsg>(new ScenarioRequestMsgData()));
        }

        public void SendCraftLibraryRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<CraftLibraryCliMsg>(new CraftLibraryListRequestMsgData { PlayerName = SettingsSystem.CurrentSettings.PlayerName }));
        }

        public void SendChatRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<ChatCliMsg>(new ChatListRequestMsgData {From = SettingsSystem.CurrentSettings.PlayerName }));
        }

        public void SendLocksRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<LockCliMsg>(new LockListRequestMsgData()));
        }

        public void SendAdminsRequest()
        {
            System.QueueOutgoingMessage(MessageFactory.CreateNew<AdminCliMsg>(new AdminListRequestMsgData()));
        }
    }
}