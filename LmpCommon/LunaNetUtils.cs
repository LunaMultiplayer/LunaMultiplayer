using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace LmpCommon
{
    public class LunaNetUtils
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

        public static IPAddress GetOwnInternalIpAddress()
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

        public static IPEndPoint CreateEndpointFromString(string endpoint)
        {
            try
            {
                if (IPAddress.TryParse(endpoint.Split(':')[0].Trim(), out var ip))
                {
                    return new IPEndPoint(ip, int.Parse(endpoint.Split(':')[1].Trim()));
                }

                var dnsIp = Dns.GetHostAddresses(endpoint.Split(':')[0].Trim());
                var port = int.Parse(endpoint.Split(':')[1].Trim());
                var ipv4Address = dnsIp.FirstOrDefault(d => d.AddressFamily == AddressFamily.InterNetwork);
                return ipv4Address != null ? new IPEndPoint(ipv4Address, port) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IPAddress CreateAddressFromString(string ipAddress)
        {
            try
            {
                if (IPAddress.TryParse(ipAddress, out var ip))
                {
                    return ip;
                }

                return Dns.GetHostAddresses(ipAddress)[0];
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
