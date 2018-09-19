using LmpClient.Base;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Color;
using LmpCommon.Message.Data.Flag;
using LmpCommon.Message.Data.Groups;
using LmpCommon.Message.Data.Kerbal;
using LmpCommon.Message.Data.PlayerStatus;
using LmpCommon.Message.Data.Scenario;
using LmpCommon.Message.Data.Settings;
using LmpCommon.Message.Data.Warp;

namespace LmpClient.Network
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
