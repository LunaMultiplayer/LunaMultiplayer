using System.Threading;
using LmpCommon;
using LmpCommon.Message.Data.MasterServer;
using LmpCommon.Message.MasterServer;
using LmpCommon.RepoRetrievers;
using LmpCommon.Time;
using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server.Server
{
    public class LidgrenMasterServer
    {
        private static int MasterServerRegistrationMsInterval => MasterServerSettings.SettingsStore.MasterServerRegistrationMsInterval < 5000 ?
            5000 : MasterServerSettings.SettingsStore.MasterServerRegistrationMsInterval;

        public static ConcurrentBag<IPEndPoint> DetectedSTUNTransportAddresses { get; private set; } = new();
        public static readonly SemaphoreSlim ReceiveSTUNResponses = new(0, 1);

        public static async void RegisterWithMasterServer()
        {
            if (!MasterServerSettings.SettingsStore.RegisterWithMasterServer) return;

            LunaLog.Normal("Master server registration is active");

            var addr4 = LunaNetUtils.GetOwnInternalIPv4Address();
            // As of right now the internal endpoint for IPv4 is mandatory, because if there is none, there is no
            // IPv4 connectivity at all, which is required to connect to the master servers (so they can determine
            // the public IPv4 address).
            if (addr4 == null) return;
            var endpoint4 = new IPEndPoint(addr4, ServerContext.Config.Port);
            // Only send IPv6 address if actually listening on IPv6, otherwise send loopback with means "none".
            IPAddress addr6;
            IPEndPoint endpoint6;
            if (LidgrenServer.Server.Socket.AddressFamily == AddressFamily.InterNetworkV6)
            {
                addr6 = LunaNetUtils.GetOwnInternalIPv6Address();
                endpoint6 = new IPEndPoint(addr6, ServerContext.Config.Port);
            }
            else
            {
                endpoint6 = new IPEndPoint(IPAddress.IPv6Loopback, ServerContext.Config.Port);
            }

            while (ServerContext.ServerRunning)
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<MsRegisterServerMsgData>();
                msgData.Id = LidgrenServer.Server.UniqueIdentifier;
                msgData.Password = !string.IsNullOrEmpty(GeneralSettings.SettingsStore.Password);
                msgData.Cheats = GeneralSettings.SettingsStore.Cheats;
                msgData.Description = GeneralSettings.SettingsStore.Description;
                msgData.CountryCode = GeneralSettings.SettingsStore.CountryCode;
                msgData.Website = GeneralSettings.SettingsStore.Website;
                msgData.WebsiteText = GeneralSettings.SettingsStore.WebsiteText;
                msgData.RainbowEffect = DedicatedServerSettings.SettingsStore.UseRainbowEffect;
                msgData.Color = new[] { DedicatedServerSettings.SettingsStore.Red, DedicatedServerSettings.SettingsStore.Green, DedicatedServerSettings.SettingsStore.Blue };
                msgData.GameMode = (int)GeneralSettings.SettingsStore.GameMode;
                msgData.InternalEndpoint = endpoint4;
                msgData.InternalEndpoint6 = endpoint6;
                msgData.MaxPlayers = GeneralSettings.SettingsStore.MaxPlayers;
                msgData.ModControl = GeneralSettings.SettingsStore.ModControl;
                msgData.PlayerCount = ServerContext.Clients.Count;
                msgData.ServerName = GeneralSettings.SettingsStore.ServerName;
                msgData.ServerVersion = LmpVersioning.CurrentVersion.ToString(3);
                msgData.VesselPositionUpdatesMsInterval = IntervalSettings.SettingsStore.VesselUpdatesMsInterval;
                msgData.SecondaryVesselPositionUpdatesMsInterval = IntervalSettings.SettingsStore.SecondaryVesselUpdatesMsInterval;
                msgData.WarpMode = (int)WarpSettings.SettingsStore.WarpMode;
                msgData.TerrainQuality = (int)GeneralSettings.SettingsStore.TerrainQuality;

                msgData.Description = msgData.Description.Length > 200 ? msgData.Description.Substring(0, 200) : msgData.Description;
                msgData.CountryCode = msgData.CountryCode.Length > 2 ? msgData.CountryCode.Substring(0, 2) : msgData.CountryCode;
                msgData.Website = msgData.Website.Length > 60 ? msgData.Website.Substring(0, 60) : msgData.Website;
                msgData.WebsiteText = msgData.WebsiteText.Length > 15 ? msgData.WebsiteText.Substring(0, 15) : msgData.WebsiteText;
                msgData.ServerName = msgData.ServerName.Length > 30 ? msgData.ServerName.Substring(0, 30) : msgData.ServerName;

                foreach (var masterServer in GetMasterServers())
                {
                    RegisterWithMasterServer(msgData, masterServer);
                }

                await Task.Delay(MasterServerRegistrationMsInterval);
            }
        }

        private static void RegisterWithMasterServer(MsRegisterServerMsgData msgData, IPEndPoint masterServer)
        {
            Task.Run(() =>
            {
                var msg = ServerContext.MasterServerMessageFactory.CreateNew<MainMstSrvMsg>(msgData);
                msg.Data.SentTime = LunaNetworkTime.UtcNow.Ticks;

                try
                {
                    var outMsg = LidgrenServer.Server.CreateMessage(msg.GetMessageSize());
                    msg.Serialize(outMsg);
                    LidgrenServer.Server.SendUnconnectedMessage(outMsg, masterServer);

                    //Force send of packets
                    LidgrenServer.Server.FlushSendQueue();
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }

        /// <summary>
        /// Check the NAT type using a STUN-like method, that is
        /// to ask all master servers for their perceived transport address (IP address and port)
        /// of our message and comparing them.
        /// If they differ it means this server is behind symmetric NAT.
        /// </summary
        public static async void CheckNATType()
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<MsSTUNBindingRequestMsgData>();
            msgData.SentTime = LunaNetworkTime.UtcNow.Ticks;
            // Signal receive loop to process STUN responses
            ReceiveSTUNResponses.Release(1);

            foreach (var masterServer in GetMasterServers())
            {
                var msg = ServerContext.ServerMessageFactory.CreateNew<MainMstSrvMsg>(msgData);
                var outMsg = LidgrenServer.Server.CreateMessage(msg.GetMessageSize());

                msg.Serialize(outMsg);
                LidgrenServer.Server.SendUnconnectedMessage(outMsg, masterServer);

                //Force send of packets
                LidgrenServer.Server.FlushSendQueue();
            }

            // Wait two seconds for responses to arrive, then wait for any in-flight message to finish processing,
            await Task.Delay(2000);
            ReceiveSTUNResponses.Wait();

            var distinctAddresses = DetectedSTUNTransportAddresses.Distinct();
            LunaLog.Debug("Detected NAT addresses: " + string.Join(", ", distinctAddresses.Select(a => a.ToString())));

            var numberDistinct = distinctAddresses.Count();
            if (numberDistinct > 1) {
                LunaLog.Error("Symmetric NAT detected. " +
                              "Players will not be able to join this server unless port forwarding is set up through UPnP or manually.");
            }
            DetectedSTUNTransportAddresses = null;
        }

        private static IPEndPoint[] GetMasterServers()
        {
            IPEndPoint[] masterServers;
            if (string.IsNullOrEmpty(DebugSettings.SettingsStore.CustomMasterServer))
                masterServers = MasterServerRetriever.MasterServers.GetValues;
            else
            {
                masterServers = new[]
                {
                    LunaNetUtils.CreateEndpointFromString(DebugSettings.SettingsStore.CustomMasterServer)
                };

            }
            return masterServers;
        }
    }
}
