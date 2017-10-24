using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Data.Warp;

namespace LunaClient.Network
{
    public class NetworkSimpleMessageSender
    {
        public static void SendMotdRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<MotdCliMsg>(new MotdRequestMsgData())));
        }

        public static void SendKerbalsRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<KerbalCliMsg>(new KerbalsRequestMsgData())));
        }

        public static void SendVesselListRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<VesselCliMsg>(new VesselListRequestMsgData())));
        }

        public static void SendSettingsRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<SettingsCliMsg>(new SettingsRequestMsgData())));
        }

        public static void SendWarpSubspacesRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<WarpCliMsg>(new WarpSubspacesRequestMsgData())));
        }

        public static void SendColorsRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<PlayerColorCliMsg>(new PlayerColorRequestMsgData())));
        }

        public static void SendPlayersRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<PlayerStatusCliMsg>(new PlayerStatusRequestMsgData())));
        }

        public static void SendScenariosRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<ScenarioCliMsg>(new ScenarioRequestMsgData())));
        }

        public static void SendCraftLibraryRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<CraftLibraryCliMsg>(new CraftLibraryListRequestMsgData { PlayerName = SettingsSystem.CurrentSettings.PlayerName })));
        }

        public static void SendChatRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<ChatCliMsg>(new ChatListRequestMsgData { From = SettingsSystem.CurrentSettings.PlayerName })));
        }

        public static void SendLocksRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<LockCliMsg>(new LockListRequestMsgData())));
        }

        public static void SendAdminsRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<AdminCliMsg>(new AdminListRequestMsgData())));
        }

        public static void SendGroupListRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<GroupCliMsg>(new GroupListRequestMsgData())));
        }
    }
}