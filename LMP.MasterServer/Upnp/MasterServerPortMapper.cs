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

        private static readonly Mapping MasterServerPortMapping = new Mapping(Protocol.Udp, Lidgren.MasterServer.Port, Lidgren.MasterServer.Port, "LMP MasterServer");
        private static readonly Mapping MasterServerWebPortMapping = new Mapping(Protocol.Tcp, LunaHttpServer.Port, LunaHttpServer.Port, "LMP MasterServer WEB");
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
    }
}
