using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace LMP.MasterServer
{
    public class Helper
    {
        public static string GetOwnIpAddress()
        {
            var currentIpAddress = TryGetIpAddress("http://ip.42.pl/raw");

            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("https://api.ipify.org/");
            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("http://httpbin.org/ip");
            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("http://checkip.dyndns.org");

            return currentIpAddress;
        }

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
    }
}
