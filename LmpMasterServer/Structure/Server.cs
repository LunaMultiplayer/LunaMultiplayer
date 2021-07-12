using LmpCommon;
using LmpCommon.Message.Data.MasterServer;
using LmpCommon.Time;
using LmpMasterServer.Dedicated;
using LmpMasterServer.Geolocalization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LmpMasterServer.Structure
{
    public class Server : ServerInfo
    {
        private static IEnumerable<string> _countryCodes;
        private static IEnumerable<string> CountryCodes => _countryCodes ?? (_countryCodes = CultureInfo
                           .GetCultures(CultureTypes.SpecificCultures)
                           .Select(culture => new RegionInfo(culture.LCID))
                           .Select(ri => ri.TwoLetterISORegionName).Distinct());

        private static readonly TimeSpan MaxCountryRequestTimeMs = TimeSpan.FromSeconds(5);
        private static DateTime _lastCountryRequestTime = DateTime.MinValue;

        private static readonly TimeoutConcurrentDictionary<IPEndPoint, string> EndpointCountries =
            new TimeoutConcurrentDictionary<IPEndPoint, string>(TimeSpan.FromHours(24).TotalMilliseconds);


        private volatile bool _refreshingCountryCode;
        public long LastRegisterTime { get; private set; }

        public void Update(MsRegisterServerMsgData msg)
        {
            InternalEndpoint = msg.InternalEndpoint;
            InternalEndpoint6 = msg.InternalEndpoint6;
            LastRegisterTime = LunaNetworkTime.UtcNow.Ticks;
            Cheats = msg.Cheats;
            Password = msg.Password;
            ServerVersion = msg.ServerVersion;
            ServerName = msg.ServerName.Length > 30 ? msg.ServerName.Substring(0, 30) : msg.ServerName;
            Description = msg.Description.Length > 200 ? msg.Description.Substring(0, 200) : msg.Description;

            if (!string.IsNullOrEmpty(msg.CountryCode) && CountryCodes.Contains(msg.CountryCode.ToUpper()))
                Country = msg.CountryCode.ToUpper();

            Website = msg.Website.Length > 60 ? msg.Website.Substring(0, 60) : msg.Website;
            WebsiteText = msg.WebsiteText.Length > 15 ? msg.WebsiteText.Substring(0, 15) : msg.WebsiteText;
            RainbowEffect = msg.RainbowEffect;
            Array.Copy(msg.Color, Color, 3);
            GameMode = msg.GameMode;
            MaxPlayers = msg.MaxPlayers;
            ModControl = msg.ModControl;
            DedicatedServer = DedicatedServerRetriever.IsDedicatedServer(ExternalEndpoint);
            PlayerCount = msg.PlayerCount;
            WarpMode = msg.WarpMode;
            TerrainQuality = msg.TerrainQuality;

            if (string.IsNullOrEmpty(Country))
                SetCountryFromEndpoint(this, ExternalEndpoint);

            if (!Website.Contains("://"))
            {
                Website = "http://" + Website;
            }

            if (string.IsNullOrEmpty(WebsiteText) && !string.IsNullOrEmpty(Website))
            {
                WebsiteText = "URL";
            }
        }

        public Server(MsRegisterServerMsgData msg, IPEndPoint externalEndpoint)
        {
            Id = msg.Id;
            ExternalEndpoint = IsLocalIpAddress(externalEndpoint.Address) ?
                new IPEndPoint(LunaNetUtils.GetOwnExternalIpAddress(), externalEndpoint.Port) :
                externalEndpoint;

            Update(msg);
        }

        private void SetCountryFromEndpoint(ServerInfo server, IPEndPoint externalEndpoint)
        {
            Task.Run(async () =>
            {
                if (_refreshingCountryCode) return;

                _refreshingCountryCode = true;
                try
                {
                    if (DateTime.UtcNow - _lastCountryRequestTime < MaxCountryRequestTimeMs)
                        Thread.Sleep(MaxCountryRequestTimeMs);

                    _lastCountryRequestTime = DateTime.UtcNow;
                    if (EndpointCountries.TryGet(externalEndpoint, out var countryCode))
                    {
                        server.Country = countryCode;
                    }
                    else
                    {
                        server.Country = await IpApi.GetCountry(externalEndpoint);
                        if (string.IsNullOrEmpty(server.Country))
                            server.Country = await IpLocate.GetCountry(externalEndpoint);

                        if (!string.IsNullOrEmpty(server.Country))
                            EndpointCountries.TryAdd(externalEndpoint, server.Country);
                    }
                }
                finally
                {
                    _refreshingCountryCode = false;
                }
            });
        }

        public static bool IsLocalIpAddress(IPAddress host)
        {
            try
            {
                var hostIPs = Dns.GetHostAddresses(host.ToString());
                var localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (var hostIp in hostIPs)
                {
                    if (IPAddress.IsLoopback(hostIp)) return true;
                    if (localIPs.Any(l => l.Equals(hostIp))) return true;
                }

                /* The private address ranges are defined in RFC1918. They are:
                 * 10.0.0.0 - 10.255.255.255 (10/8 prefix)
                 * 172.16.0.0 - 172.31.255.255 (172.16/12 prefix)
                 * 192.168.0.0 - 192.168.255.255 (192.168/16 prefix)
                 */

                var bytes = host.GetAddressBytes();
                switch (bytes[0])
                {
                    case 10:
                        return true;
                    case 172:
                        return bytes[1] < 32 && bytes[1] >= 16;
                    case 192:
                        return bytes[1] == 168;
                    default:
                        return false;
                }
            }
            catch
            {
                // ignored
            }
            return false;
        }
    }
}
