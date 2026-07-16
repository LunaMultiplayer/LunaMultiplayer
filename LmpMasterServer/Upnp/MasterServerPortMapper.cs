using LmpMasterServer.Http;
using Microsoft.VisualStudio.Threading;
using Mono.Nat;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LmpMasterServer.Upnp
{
    public class MasterServerPortMapper
    {
        public static bool UseUpnp { get; set; } = true;

        private static readonly int LifetimeInSeconds = (int)TimeSpan.FromMinutes(1).TotalSeconds;
        private static readonly AsyncLazy<INatDevice> Device = new AsyncLazy<INatDevice>(DiscoverDeviceAsync, new JoinableTaskContext().Factory);

        private static Mapping MasterServerPortMapping => new Mapping(Protocol.Udp, Lidgren.MasterServer.Port, Lidgren.MasterServer.Port, LifetimeInSeconds, $"LMPMasterSrv {Lidgren.MasterServer.Port}");
        private static Mapping MasterServerWebPortMapping => new Mapping(Protocol.Tcp, LunaHttpServer.Port, LunaHttpServer.Port, LifetimeInSeconds, $"LMPMasterSrvWeb {LunaHttpServer.Port}");

        private static async Task<INatDevice> DiscoverDeviceAsync()
        {
            var tcs = new TaskCompletionSource<INatDevice>();
            var cts = new CancellationTokenSource(5000);
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

        [DebuggerHidden]
        public static async Task OpenPortAsync()
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

        [DebuggerHidden]
        public static async Task RemoveOpenedPortsAsync()
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
        public static async Task RefreshUpnpPortAsync()
        {
            if (UseUpnp)
            {
                while (Lidgren.MasterServer.RunServer)
                {
                    await OpenPortAsync();
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            }
        }
    }
}
