using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace LmpCommon
{
    public static class LunaNetUtils
    {
        public static bool IsTcpPortInUse(int port)
        {
            try
            {
                return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(e => e.Port == port);
            }
            catch (Exception)
            {
                //This fails on macOS High Sierra
                return false;
            }
        }

        public static bool IsUdpPortInUse(int port)
        {
            try
            {
                return IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Any(e => e.Port == port);
            }
            catch (Exception)
            {
                //This fails on macOS High Sierra
                return false;
            }
        }

        public static IPAddress GetOwnSubnetMask()
        {
            var ni = GetNetworkInterface();
            if (ni == null)
            {
                return IPAddress.Any;
            }

            var properties = ni.GetIPProperties();
            foreach (var unicastAddress in properties.UnicastAddresses)
            {
                if (unicastAddress?.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return unicastAddress.IPv4Mask;
                }
            }

            return IPAddress.Any;
        }

        public static IPAddress GetOwnInternalIPv4Address()
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

        public static UnicastIPAddressInformation GetOwnInternalIPv6Network()
        {
            var ni = GetNetworkInterface();
            if (ni == null)
            {
                return null;
            }

            var properties = ni.GetIPProperties();
            foreach (var unicastAddress in properties.UnicastAddresses)
            {
                if (unicastAddress?.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetworkV6
                                                    && !unicastAddress.Address.IsIPv6UniqueLocal() && !unicastAddress.Address.IsIPv6LinkLocal
                                                    && !unicastAddress.Address.IsIPv6SiteLocal && !unicastAddress.Address.IsIPv6Teredo)
                {
                    return unicastAddress;
                }
            }

            return null;
        }

        public static IPAddress GetOwnInternalIPv6Address()
        {
            var info = GetOwnInternalIPv6Network();
            if (info == null)
                return IPAddress.IPv6Loopback;

            return info.Address;
        }

        /// <summary>
        /// Gets whether the address is an IPv6 Unique Local address.
        /// Backport of https://github.com/dotnet/runtime/pull/48853 landing in .NET 6
        /// </summary>
        private static bool IsIPv6UniqueLocal(this IPAddress address)
        {
            var firstBytes = (ushort)address.GetAddressBytes()[0];
            return address.AddressFamily == AddressFamily.InterNetworkV6 && (firstBytes & 0xFE00) == 0xFC00;
        }

        public static IPAddress GetOwnExternalIpAddress()
        {
            var currentIpAddress = TryGetIpAddress("http://ip.42.pl/raw");

            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("https://api.ipify.org/");
            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("http://httpbin.org/ip");
            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("http://checkip.dyndns.org");

            return IPAddress.TryParse(currentIpAddress, out var ipAddress) ? ipAddress : null;
        }

        // TODO IPv6: This does not return AAAA records of hostnames.
        // However it is only used to parse the dedicated and master server list, which for one is mostly hostnames,
        // and tight now we rely on server/client<->master server connections being done IPv4-only, so the master
        // server gets the public IPv4 address.
        public static IPEndPoint CreateEndpointFromString(string endpoint)
        {
            try
            {
                    // [2001:db8::1]:8800
                    // 192.0.2.1:8800
                    var indexOfPortSeparator = endpoint.LastIndexOf(":", StringComparison.Ordinal);
                    var ip = endpoint.Substring(0, indexOfPortSeparator);
                    var port = int.Parse(endpoint.Substring(indexOfPortSeparator + 1));
                    if (IPAddress.TryParse(ip, out var addr))
                        return new IPEndPoint(addr, port);

                    var dnsIp = Dns.GetHostAddresses(ip.Trim());
                    var ipv4Address = dnsIp.FirstOrDefault(d => d.AddressFamily == AddressFamily.InterNetwork);
                    return ipv4Address != null ? new IPEndPoint(ipv4Address, port) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IPAddress[] CreateAddressFromString(string address)
        {
            try
            {
                if (IPAddress.TryParse(address, out var ip))
                {
                    return new []{ ip };
                }
                return Dns.GetHostAddresses(address);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #region Private

        private static string TryGetIpAddress(string url)
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead(url))
                {
                    if (stream == null) return null;
                    using (var reader = new StreamReader(stream))
                    {
                        var ipRegEx = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                        var result = ipRegEx.Matches(reader.ReadToEnd());

                        if (IPAddress.TryParse(result[0].Value, out var ip))
                            return ip.ToString();
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

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

        #endregion
    }
}
