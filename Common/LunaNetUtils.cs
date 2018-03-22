using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LunaCommon
{
    public class LunaNetUtils
    {
        private static NetworkInterface GetNetworkInterface()
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            if (nics.Length < 1)
                return null;

            NetworkInterface best = null;
            foreach (var adapter in nics)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback || adapter.NetworkInterfaceType == NetworkInterfaceType.Unknown)
                    continue;
                if (!adapter.Supports(NetworkInterfaceComponent.IPv4))
                    continue;
                if (best == null)
                    best = adapter;
                if (adapter.OperationalStatus != OperationalStatus.Up)
                    continue;

                // make sure this adapter has any ipv4 addresses
                var properties = adapter.GetIPProperties();
                foreach (var unicastAddress in properties.UnicastAddresses)
                {
                    if (unicastAddress?.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // Yes it does, return this network interface.
                        return adapter;
                    }
                }
            }
            return best;
        }

        public static IPAddress GetMyAddress()
        {
            var ni = GetNetworkInterface();
            if (ni == null)
            {
                return IPAddress.Loopback;
            }

            var properties = ni.GetIPProperties();
            foreach (var unicastAddress in properties.UnicastAddresses)
            {
                if (unicastAddress?.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return unicastAddress.Address;
                }
            }
            
            return IPAddress.Loopback;
        }
    }
}
