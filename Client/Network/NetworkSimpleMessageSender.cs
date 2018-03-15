using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Data.Warp;

namespace LunaClient.Network
{
    public class NetworkSimpleMessageSender
    {
        public static void SendHandshakeRequest()
        {
            NetworkSender.OutgoingMessages.Enqueue(NetworkMain.CliMsgFactory.CreateNew<HandshakeCliMsg, HandshakeRequestMsgData>());
        }

        public static void SendMotdRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<MotdCliMsg, MotdRequestMsgData>()));
        }

        public static void SendKerbalsRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<KerbalCliMsg, KerbalsRequestMsgData>()));
        }
        
        public static void SendSettingsRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<SettingsCliMsg, SettingsRequestMsgData>()));
        }

        public static void SendWarpSubspacesRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<WarpCliMsg, WarpSubspacesRequestMsgData>()));
        }

        public static void SendColorsRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<PlayerColorCliMsg, PlayerColorRequestMsgData>()));
        }

        public static void SendFlagsRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<FlagCliMsg, FlagListRequestMsgData>()));
        }

        public static void SendPlayersRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<PlayerStatusCliMsg, PlayerStatusRequestMsgData>()));
        }

        public static void SendScenariosRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<ScenarioCliMsg, ScenarioRequestMsgData>()));
        }

        public static void SendCraftLibraryRequest()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryListRequestMsgData>();
            msgData.PlayerName = SettingsSystem.CurrentSettings.PlayerName;

            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<CraftLibraryCliMsg>(msgData)));
        }

        public static void SendAdminsRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<AdminCliMsg, AdminListRequestMsgData>()));
        }

        public static void SendGroupListRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<GroupCliMsg, GroupListRequestMsgData>()));
        }
    }
}