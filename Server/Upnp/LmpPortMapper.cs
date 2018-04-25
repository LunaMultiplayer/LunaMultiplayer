using Open.Nat;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Upnp
{
    public class LmpPortMapper
    {
        private static readonly Mapping LmpPortMapping = new Mapping(Protocol.Udp, ConnectionSettings.SettingsStore.Port, ConnectionSettings.SettingsStore.Port, "LMP Server");
        private static readonly Lazy<NatDevice> Device = new Lazy<NatDevice>(DiscoverDevice);

        private static NatDevice DiscoverDevice()
        {
            var nat = new NatDiscoverer();
            return nat.DiscoverDeviceAsync(PortMapper.Upnp, new CancellationTokenSource(ConnectionSettings.SettingsStore.UpnpMsTimeout)).Result;
        }

        public static async Task OpenPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp)
            {
                try
                {
                    await Device.Value.CreatePortMapAsync(LmpPortMapping);
                    LunaLog.Debug($"UPnP active. Port: {ConnectionSettings.SettingsStore.Port} opened!");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public static async Task RemoveOpenedPorts()
        {
            if (ConnectionSettings.SettingsStore.Upnp)
            {
                try
                {
                    await Device.Value.DeletePortMapAsync(LmpPortMapping);
                    LunaLog.Debug($"UPnP active. Port: {ConnectionSettings.SettingsStore.Port} closed!");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public static async Task<IPAddress> GetExternalIp()
        {
            return await Device.Value.GetExternalIPAsync();
        }
    }
}
