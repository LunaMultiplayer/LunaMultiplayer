using LmpMasterServer.Http;
using Microsoft.VisualStudio.Threading;
using Open.Nat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LmpMasterServer.Upnp
{
    public class MasterServerPortMapper
    {
        public static bool UseUpnp { get; set; } = true;

        private static readonly int LifetimeInSeconds = (int)TimeSpan.FromMinutes(1).TotalSeconds;
        private static readonly AsyncLazy<NatDevice> Device = new AsyncLazy<NatDevice>(DiscoverDevice);

        private static Mapping MasterServerPortMapping => new Mapping(Protocol.Udp, Lidgren.MasterServer.Port, Lidgren.MasterServer.Port, LifetimeInSeconds, $"LMPMasterSrv {Lidgren.MasterServer.Port}");
        private static Mapping MasterServerWebPortMapping => new Mapping(Protocol.Tcp, LunaHttpServer.Port, LunaHttpServer.Port, LifetimeInSeconds, $"LMPMasterSrvWeb {LunaHttpServer.Port}");

        private static async Task<NatDevice> DiscoverDevice()
        {
            var nat = new NatDiscoverer();
            return await nat.DiscoverDeviceAsync(PortMapper.Upnp, new CancellationTokenSource(5000));
        }

        public static async Task OpenPort()
        {
            if (UseUpnp)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.CreatePortMapAsync(MasterServerPortMapping);
                    await device.CreatePortMapAsync(MasterServerWebPortMapping);
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
                    var device = await Device.GetValueAsync();
                    await device.DeletePortMapAsync(MasterServerPortMapping);
                    await device.DeletePortMapAsync(MasterServerWebPortMapping);
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
            if (UseUpnp)
            {
                while (Lidgren.MasterServer.RunServer)
                {
                    await OpenPort();
                    await Task.Delay((int)TimeSpan.FromSeconds(30).TotalMilliseconds);
                }
            }
        }
    }
}
