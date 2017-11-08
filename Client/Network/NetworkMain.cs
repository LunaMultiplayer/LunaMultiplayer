using Lidgren.Network;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LunaClient.Network
{
    public class NetworkMain
    {
        public static ClientMessageFactory CliMsgFactory { get; } = new ClientMessageFactory(SettingsSystem.CurrentSettings.CompressionEnabled);
        public static ServerMessageFactory SrvMsgFactory { get; } = new ServerMessageFactory(SettingsSystem.CurrentSettings.CompressionEnabled);
        public static MasterServerMessageFactory MstSrvMsgFactory { get; } = new MasterServerMessageFactory(SettingsSystem.CurrentSettings.CompressionEnabled);

        public static Task ReceiveThread { get; set; }
        public static Task SendThread { get; set; }

        public static NetPeerConfiguration Config { get; } = new NetPeerConfiguration("LMP")
        {
            AutoFlushSendQueue = false,
            SuppressUnreliableUnorderedAcks = true, //We don't need ack for unreliable unordered!
            MaximumTransmissionUnit = SettingsSystem.CurrentSettings.MtuSize,
            PingInterval = (float)SettingsSystem.CurrentSettings.HearbeatMsInterval / 1000,
            ConnectionTimeout = (float)SettingsSystem.CurrentSettings.ConnectionMsTimeout / 1000,
        };

        public static NetClient ClientConnection { get; private set; }

        public static void DeleteAllTheControlLocksSoTheSpaceCentreBugGoesAway()
        {
            LunaLog.Log($"[LMP]: Clearing {InputLockManager.lockStack.Count} control locks");
            InputLockManager.ClearControlLocks();
        }

        public static void ResetConnectionStaticsAndQueues()
        {
            NetworkSender.OutgoingMessages = new ConcurrentQueue<IMessageBase>();
            NetworkStatistics.LastReceiveTime = DateTime.MinValue;
            NetworkStatistics.LastSendTime = DateTime.MinValue;
        }

        public static void AwakeNetworkSystem()
        {
            Config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
            Config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            Config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            NetworkServerList.RefreshMasterServers();
            NetworkServerList.RequestServers();
        }

        public static void ResetNetworkSystem()
        {
            NetworkConnection.ResetRequested = true;

            ClientConnection = new NetClient(Config);
            ClientConnection.Start();

            if (SendThread != null && !SendThread.IsCompleted)
                SendThread?.Wait(1000);
            if (ReceiveThread != null && !ReceiveThread.IsCompleted)
                ReceiveThread?.Wait(1000);

            NetworkConnection.ResetRequested = false;

            ReceiveThread = SystemBase.LongRunTaskFactory.StartNew(NetworkReceiver.ReceiveMain);
            SendThread = SystemBase.LongRunTaskFactory.StartNew(NetworkSender.SendMain);
        }

        public static void HandleDisconnectException(Exception e)
        {
            if (e.InnerException != null)
            {
                LunaLog.LogError($"[LMP]: Connection error: {e.Message}, {e.InnerException}");
                NetworkConnection.Disconnect($"Connection error: {e.Message}, {e.InnerException.Message}");
            }
            else
            {
                LunaLog.LogError($"[LMP]: Connection error: {e}");
                NetworkConnection.Disconnect($"Connection error: {e.Message}");
            }
        }
    }
}
