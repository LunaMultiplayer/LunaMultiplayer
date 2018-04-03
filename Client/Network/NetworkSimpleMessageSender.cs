using LunaClient.Base;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Data.Settings;
using LunaCommon.Message.Data.Warp;

namespace LunaClient.Network
{
    public class NetworkSimpleMessageSender
    {
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

        public static void SendGroupListRequest()
        {
            SystemBase.TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<GroupCliMsg, GroupListRequestMsgData>()));
        }
    }
}
