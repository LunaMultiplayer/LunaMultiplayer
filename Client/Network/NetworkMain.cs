using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lidgren.Network;
using LunaClient.Systems;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Message;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Network
{
    public class NetworkMain
    {
        public static ClientMessageFactory CliMsgFactory { get; } = new ClientMessageFactory(SettingsSystem.CurrentSettings.CompressionEnabled);
        public static ServerMessageFactory SrvMsgFactory { get; } = new ServerMessageFactory(SettingsSystem.CurrentSettings.CompressionEnabled);
        public static MasterServerMessageFactory MstSrvMsgFactory { get; } = new MasterServerMessageFactory(SettingsSystem.CurrentSettings.CompressionEnabled);

        public static Task ReceiveThread { get; } = new Task(NetworkReceiver.ReceiveMain);
        public static Task SendThread { get; } = new Task(NetworkSender.SendMain);

        public static NetPeerConfiguration Config { get; } = new NetPeerConfiguration("LMP")
        {
            AutoFlushSendQueue = false,
            SuppressUnreliableUnorderedAcks = true, //We don't need ack for unreliable unordered!
            MaximumTransmissionUnit = SettingsSystem.CurrentSettings.MtuSize,
            PingInterval = (float)SettingsSystem.CurrentSettings.HearbeatMsInterval / 1000,
            ConnectionTimeout = (float)SettingsSystem.CurrentSettings.ConnectionMsTimeout / 1000
        };

        public static NetClient ClientConnection { get; } = new NetClient(Config);

        public static void DeleteAllTheControlLocksSoTheSpaceCentreBugGoesAway()
        {
            Debug.Log("Clearing " + InputLockManager.lockStack.Count + " control locks");
            InputLockManager.ClearControlLocks();
        }

        public static void ResetConnectionStaticsAndQueues()
        {
            SystemsHandler.ClearMessageQueues();
            NetworkSender.OutgoingMessages = new ConcurrentQueue<IMessageBase>();
            SettingsSystem.Singleton.ResetServerSettings();
            NetworkStatistics.LastReceiveTime = 0;
            NetworkStatistics.LastSendTime = 0;
        }

        public static void StartNetworkSystem()
        {
            NetworkServerList.RefreshMasterServers();
            SendThread.Start(TaskScheduler.Default);
            ReceiveThread.Start(TaskScheduler.Default);
            NetworkServerList.RequestServers();
        }
        
        public static void HandleDisconnectException(Exception e)
        {
            if (e.InnerException != null)
            {
                Debug.LogError("Connection error: " + e.Message + ", " + e.InnerException);
                NetworkConnection.Disconnect("Connection error: " + e.Message + ", " + e.InnerException.Message);
            }
            else
            {
                Debug.LogError("Connection error: " + e);
                NetworkConnection.Disconnect("Connection error: " + e.Message);
            }
        }
    }
}
