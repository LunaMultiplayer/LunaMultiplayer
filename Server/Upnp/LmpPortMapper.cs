using Microsoft.VisualStudio.Threading;
using Open.Nat;
using Server.Context;
using Server.Events;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Upnp
{
    public static class LmpPortMapper
    {
        private static readonly int LifetimeInSeconds = (int)TimeSpan.FromMinutes(5).TotalSeconds;
        private static readonly AsyncLazy<NatDevice> Device = new AsyncLazy<NatDevice>(DiscoverDevice);

        private static Mapping LmpPortMapping => new Mapping(Protocol.Udp, ConnectionSettings.SettingsStore.Port, ConnectionSettings.SettingsStore.Port,
            LifetimeInSeconds, $"LMPServer {ConnectionSettings.SettingsStore.Port}");

        private static Mapping LmpWebPortMapping => new Mapping(Protocol.Tcp, WebsiteSettings.SettingsStore.Port, WebsiteSettings.SettingsStore.Port,
            LifetimeInSeconds, $"LMPServerWeb {WebsiteSettings.SettingsStore.Port}");

        private static async Task<NatDevice> DiscoverDevice()
        {
            var nat = new NatDiscoverer();
            return await nat.DiscoverDeviceAsync(PortMapper.Upnp, new CancellationTokenSource(ConnectionSettings.SettingsStore.UpnpMsTimeout));
        }

        static LmpPortMapper() => ExitEvent.ServerClosing += () =>
        {
            CloseLmpPort().Wait();
            CloseWebPort().Wait();
        };

        /// <summary>
        /// Opens the port set in the settings using UPnP. With a lifetime of <see cref="LifetimeInSeconds"/> seconds
        /// </summary>
        public static async Task OpenLmpPort(bool verbose = true)
        {
            if (ConnectionSettings.SettingsStore.Upnp)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.CreatePortMapAsync(LmpPortMapping);
                    if (verbose) LunaLog.Debug($"UPnP active. Port: {ConnectionSettings.SettingsStore.Port} {LmpPortMapping.Protocol} opened!");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Opens the website port set in the settings using UPnP. With a lifetime of <see cref="LifetimeInSeconds"/> seconds
        /// </summary>
        public static async Task OpenWebPort(bool verbose = true)
        {
            if (ConnectionSettings.SettingsStore.Upnp && WebsiteSettings.SettingsStore.EnableWebsite)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.CreatePortMapAsync(LmpWebPortMapping);
                    if (verbose) LunaLog.Debug($"UPnP + Website active. Port: {WebsiteSettings.SettingsStore.Port} {LmpWebPortMapping.Protocol} opened!");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Refresh the UPnP port every 1 minute
        /// </summary>
        public static async void RefreshUpnpPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp)
            {
                while (ServerContext.ServerRunning)
                {
                    await OpenLmpPort(false);
                    await OpenWebPort(false);
                    await Task.Delay((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
                }
            }
        }

        /// <summary>
        /// Closes the opened port using UPnP
        /// </summary>
        public static async Task CloseLmpPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp && ServerContext.ServerRunning)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.DeletePortMapAsync(LmpPortMapping);
                    LunaLog.Debug($"UPnP active. Port: {ConnectionSettings.SettingsStore.Port} {LmpPortMapping.Protocol} closed!");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Closes the opened web port using UPnP
        /// </summary>
        public static async Task CloseWebPort()
        {
            if (ConnectionSettings.SettingsStore.Upnp && WebsiteSettings.SettingsStore.EnableWebsite && ServerContext.ServerRunning)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.DeletePortMapAsync(LmpWebPortMapping);
                    LunaLog.Debug($"UPnP + Website active. Port: {WebsiteSettings.SettingsStore.Port} {LmpWebPortMapping.Protocol} closed!");
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Gets external IP using UPnP
        /// </summary>
        public static async Task<IPAddress> GetExternalIp()
        {
            var device = await Device.GetValueAsync();
            return await device.GetExternalIPAsync();
        }
    }
}
