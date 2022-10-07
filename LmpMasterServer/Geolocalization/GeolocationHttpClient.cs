using System;
using System.Net.Http;

namespace LmpMasterServer.Geolocalization
{
    public static class GeolocationHttpClient
    {
        private static HttpClient _client;

        public static HttpClient GetClient()
        {
            if (_client == null)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; LunaMultiplayer Master Server)");
                client.Timeout = TimeSpan.FromSeconds(10);
                _client = client;
            }
            return _client;
        }
    }
}
