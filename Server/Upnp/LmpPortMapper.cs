using Microsoft.VisualStudio.Threading;
using Mono.Nat;
using Server.Context;
using Server.Events;
using Server.Log;
using Server.Settings.Structures;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Upnp
{
    public static class LmpPortMapper
    {
        private static readonly int LifetimeInSeconds = (int)TimeSpan.FromMinutes(5).TotalSeconds;
        private static readonly AsyncLazy<INatDevice> Device = new AsyncLazy<INatDevice>(DiscoverDeviceAsync, new JoinableTaskContext().Factory);

        private static Mapping LmpPortMapping => new Mapping(Protocol.Udp, ConnectionSettings.SettingsStore.Port, ConnectionSettings.SettingsStore.Port,
            LifetimeInSeconds, $"LMPServer {ConnectionSettings.SettingsStore.Port}");

        private static Mapping LmpWebPortMapping => new Mapping(Protocol.Tcp, WebsiteSettings.SettingsStore.Port, WebsiteSettings.SettingsStore.Port,
            LifetimeInSeconds, $"LMPServerWeb {WebsiteSettings.SettingsStore.Port}");

        private static async Task<INatDevice> DiscoverDeviceAsync()
        {
            var tcs = new TaskCompletionSource<INatDevice>();
            var cts = new CancellationTokenSource(ConnectionSettings.SettingsStore.UpnpMsTimeout);
            cts.Token.Register(() => tcs.TrySetCanceled());

            void OnDeviceFound(object sender, DeviceEventArgs args)
            {
                tcs.TrySetResult(args.Device);
            }

            NatUtility.DeviceFound += OnDeviceFound;
            NatUtility.StartDiscovery();

            try
            {
                return await tcs.Task;
            }
            finally
            {
                NatUtility.StopDiscovery();
                NatUtility.DeviceFound -= OnDeviceFound;
            }
        }

        static LmpPortMapper() => ExitEvent.ServerClosing += () =>
        {
            _ = Task.Run(async () =>
            {
                await CloseLmpPortAsync();
                await CloseWebPortAsync();
            });
        };

        /// <summary>
        /// Opens the port set in the settings using UPnP. With a lifetime of <see cref="LifetimeInSeconds"/> seconds
        /// </summary>
        [DebuggerHidden]
        public static async Task OpenLmpPortAsync(bool verbose = true)
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
        [DebuggerHidden]
        public static async Task OpenWebPortAsync(bool verbose = true)
        {
            if (ConnectionSettings.SettingsStore.Upnp && WebsiteSettings.SettingsStore.EnableWebsite)
            {
                try
                {
                    var device = await Device.GetValueAsync();
                    await device.CreatePortMapAsync(LmpWebPortMapping);
                    if (verbose) LunaLog.Debug($"UPnP for website active. Port: {WebsiteSettings.SettingsStore.Port} {LmpWebPortMapping.Protocol} opened!");
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
        public static async Task RefreshUpnpPortAsync()
        {
            if (ConnectionSettings.SettingsStore.Upnp)
            {
                while (ServerContext.ServerRunning)
                {
                    await OpenLmpPortAsync(false);
                    await OpenWebPortAsync(false);
                    await Task.Delay((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
                }
            }
        }

        /// <summary>
        /// Closes the opened port using UPnP
        /// </summary>
        [DebuggerHidden]
        public static async Task CloseLmpPortAsync()
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
        [DebuggerHidden]
        public static async Task CloseWebPortAsync()
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
        public static async Task<IPAddress> GetExternalIpAsync()
        {
            var device = await Device.GetValueAsync();
            return await device.GetExternalIPAsync();
        }
    }
}
