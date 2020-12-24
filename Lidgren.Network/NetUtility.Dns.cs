#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
using NetAddress = System.Net.IPAddress;
#endif

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Lidgren.Network
{
    public static partial class NetUtility
    {
        /// <summary>
        /// Resolve endpoint callback
        /// </summary>
        public delegate void ResolveEndPointCallback(NetEndPoint endPoint);

        /// <summary>
        /// Resolve address callback
        /// </summary>
        public delegate void ResolveAddressCallback(NetAddress adr);

        /// <summary>
        /// Get IPv4 or IPv6 address from notation (xxx.xxx.xxx.xxx or xxxx:xxxx:...:xxxx) or hostname (asynchronous version)
        /// </summary>
        public static void ResolveAsync(string ipOrHost, int port, ResolveEndPointCallback callback)
        {
            ResolveAsync(ipOrHost, port, null, callback);
        }

        public static void ResolveAsync(string ipOrHost, int port, AddressFamily? allowedFamily,
            ResolveEndPointCallback callback)
        {
            ResolveAsync(ipOrHost, allowedFamily, delegate(NetAddress adr)
            {
                if (adr == null)
                {
                    callback(null);
                }
                else
                {
                    callback(new NetEndPoint(adr, port));
                }
            });
        }

        /// <summary>
        /// Get IPv4 or IPv6 address from notation (xxx.xxx.xxx.xxx or xxxx:xxxx:...:xxxx) or hostname
        /// </summary>
        public static NetEndPoint Resolve(string ipOrHost, int port)
        {
            return Resolve(ipOrHost, port, null);
        }

        public static NetEndPoint Resolve(string ipOrHost, int port, AddressFamily? allowedFamily)
        {
            var adr = Resolve(ipOrHost, allowedFamily);
            return adr == null ? null : new NetEndPoint(adr, port);
        }

        /// <summary>
        /// Get IPv4 or IPv6 address from notation (xxx.xxx.xxx.xxx or xxxx:xxxx:...:xxxx) or hostname (asynchronous version)
        /// </summary>
        public static void ResolveAsync(string ipOrHost, ResolveAddressCallback callback)
        {
            ResolveAsync(ipOrHost, null, callback);
        }

        /// <summary>
        /// Get IPv4 or IPv6 address from notation (xxx.xxx.xxx.xxx or xxxx:xxxx:...:xxxx) or hostname (asynchronous version)
        /// </summary>
        public static void ResolveAsync(string ipOrHost, AddressFamily? allowedFamily, ResolveAddressCallback callback)
        {
            if (ResolveHead(ref ipOrHost, allowedFamily, out var resolve))
            {
                callback(resolve);
                return;
            }

            // ok must be a host name
            IPHostEntry entry;
            try
            {
                Dns.BeginGetHostEntry(ipOrHost, delegate(IAsyncResult result)
                {
                    try
                    {
                        entry = Dns.EndGetHostEntry(result);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.HostNotFound)
                        {
                            //LogWrite(string.Format(CultureInfo.InvariantCulture, "Failed to resolve host '{0}'.", ipOrHost));
                            callback(null);
                            return;
                        }
                        else
                        {
                            throw;
                        }
                    }

                    if (entry == null)
                    {
                        callback(null);
                        return;
                    }

                    // check each entry for a valid IP address
                    ResolveFilter(allowedFamily, entry.AddressList);
                }, null);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.HostNotFound)
                {
                    //LogWrite(string.Format(CultureInfo.InvariantCulture, "Failed to resolve host '{0}'.", ipOrHost));
                    callback(null);
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task<NetEndPoint> ResolveAsync(string ipOrHost, int port)
        {
            return await ResolveAsync(ipOrHost, port, (AddressFamily?) null);
        }

        public static async Task<NetEndPoint> ResolveAsync(string ipOrHost, int port, AddressFamily? allowedFamily)
        {
            var adr = await ResolveAsync(ipOrHost, allowedFamily);
            return adr == null ? null : new NetEndPoint(adr, port);
        }

        public static async Task<NetAddress> ResolveAsync(string ipOrHost, AddressFamily? allowedFamily = null)
        {
            if (ResolveHead(ref ipOrHost, allowedFamily, out var resolve))
            {
                return resolve;
            }

            // ok must be a host name
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(ipOrHost);
                if (addresses == null)
                    return null;
                return ResolveFilter(allowedFamily, addresses);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.HostNotFound)
                {
                    //LogWrite(string.Format(CultureInfo.InvariantCulture, "Failed to resolve host '{0}'.", ipOrHost));
                    return null;
                }
                else
                {
                    throw;
                }
            }

        }

        /// <summary>
        /// Get IPv4 or IPv6 address from notation (xxx.xxx.xxx.xxx or xxxx:xxxx:...:xxxx) or hostname
        /// </summary>
        public static NetAddress Resolve(string ipOrHost)
        {
            return Resolve(ipOrHost, null);
        }


        /// <summary>
        /// Get IPv4 or IPv6 address from notation (xxx.xxx.xxx.xxx or xxxx:xxxx:...:xxxx) or hostname,
        /// taking in an allowed address family to filter resolved addresses by.
        /// </summary>
        /// <remarks>
        /// If <paramref name="allowedFamily"/> is not null, the address returned will only be of the specified family.
        /// </remarks>
        /// <param name="ipOrHost">The hostname or IP address to parse.</param>
        /// <param name="allowedFamily">If not null, the allowed address family to return.</param>
        /// <returns>
        /// A resolved address matching the specified filter if it exists,
        /// null if no such address exists or a lookup error occured.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="ipOrHost"/> is null or empty OR
        /// <paramref name="allowedFamily"/> is not one of null, <see cref="AddressFamily.InterNetwork"/>
        /// or <see cref="AddressFamily.InterNetworkV6"/>
        /// </exception>
        public static NetAddress Resolve(string ipOrHost, AddressFamily? allowedFamily)
        {
            if (ResolveHead(ref ipOrHost, allowedFamily, out var resolve))
            {
                return resolve;
            }

            // ok must be a host name
            try
            {
                var addresses = Dns.GetHostAddresses(ipOrHost);
                if (addresses == null)
                    return null;
                return ResolveFilter(allowedFamily, addresses);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.HostNotFound)
                {
                    //LogWrite(string.Format(CultureInfo.InvariantCulture, "Failed to resolve host '{0}'.", ipOrHost));
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        private static IPAddress ResolveFilter(AddressFamily? allowedFamily, IPAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                var family = address.AddressFamily;
                if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
                    continue;
                if (allowedFamily == null || allowedFamily == family)
                    return address;
            }

            return null;
        }

        private static bool ResolveHead(ref string ipOrHost, AddressFamily? allowedFamily, out IPAddress resolve)
        {
            if (string.IsNullOrEmpty(ipOrHost))
                throw new ArgumentException("Supplied string must not be empty", "ipOrHost");

            if (allowedFamily != null && allowedFamily != AddressFamily.InterNetwork
                                      && allowedFamily != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException("Address family must be either InterNetwork, InterNetworkV6 or null",
                    nameof(allowedFamily));
            }

            ipOrHost = ipOrHost.Trim();

            NetAddress ipAddress = null;
            if (NetAddress.TryParse(ipOrHost, out ipAddress))
            {
                if (allowedFamily != null && ipAddress.AddressFamily != allowedFamily)
                {
                    resolve = null;
                    return true;
                }

                if (ipAddress.AddressFamily == AddressFamily.InterNetwork ||
                    ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    resolve = ipAddress;
                    return true;
                }

                throw new ArgumentException("This method will not currently resolve other than IPv4 or IPv6 addresses");
            }

            resolve = null;
            return false;
        }
    }
}