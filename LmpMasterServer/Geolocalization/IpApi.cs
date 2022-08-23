using LmpMasterServer.Log;
using System;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace LmpMasterServer.Geolocalization
{
    internal class IpApi : IGeolocalization
    {
        public static async Task<string> GetCountryAsync(IPEndPoint externalEndpoint)
        {
            try
            {
                var client = GeolocationHttpClient.GetClient();
                var output = JsonNode.Parse(
                    await client.GetStringAsync($"https://ipapi.co/{externalEndpoint.Address}/json/"));
                return output?["country"]?.GetValue<string>();
            }
            catch (Exception e)
            {
                LunaLog.Warning(e.Message);
                return null;
            }
        }
    }
}
