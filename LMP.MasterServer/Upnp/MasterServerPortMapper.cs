using LMP.MasterServer.Http;
using Open.Nat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LMP.MasterServer.Upnp
{
    public class MasterServerPortMapper
    {
        public static bool UseUpnp { get; set; } = true;

        private static readonly int LifetimeInSeconds = (int)TimeSpan.FromMinutes(1).TotalSeconds;
        private static readonly Mapping MasterServerPortMapping = new Mapping(Protocol.Udp, Lidgren.MasterServer.Port, Lidgren.MasterServer.Port, LifetimeInSeconds, "LMP MasterServer");
        private static readonly Mapping MasterServerWebPortMapping = new Mapping(Protocol.Tcp, LunaHttpServer.Port, LunaHttpServer.Port, LifetimeInSeconds, "LMP MasterServer WEB");
        private static readonly Lazy<NatDevice> Device = new Lazy<NatDevice>(DiscoverDevice);

        private static NatDevice DiscoverDevice()
        {
            var nat = new NatDiscoverer();
            return nat.DiscoverDeviceAsync(PortMapper.Upnp, new CancellationTokenSource(5000)).Result;
        }

        public static async Task OpenPort()
        {
            if (UseUpnp)
            {
                try
                {
                    await Device.Value.CreatePortMapAsync(MasterServerPortMapping);
                    await Device.Value.CreatePortMapAsync(MasterServerWebPortMapping);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public static async Task RemoveOpenedPorts()
        {
            if (UseUpnp)
            {
                try
                {
                    await Device.Value.DeletePortMapAsync(MasterServerPortMapping);
                    await Device.Value.DeletePortMapAsync(MasterServerWebPortMapping);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Refresh the UPnP port every 30 seconds
        /// </summary>
        public static async void RefreshUpnpPort()
        {
            while (Lidgren.MasterServer.RunServer)
            {
                await OpenPort();
                await Task.Delay((int)TimeSpan.FromSeconds(30).TotalMilliseconds);
            }
        }
    }
}
