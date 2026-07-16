using LmpMasterServer.Log;
using System;
using System.Net;
using System.Net.Http;
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
                using var response = await client.GetAsync(
                    $"https://ipapi.co/{externalEndpoint.Address}/json/").ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    var code = (int)response.StatusCode;
                    // Free tier often returns 403/429 for datacenter IPs, automation, or quotas; backup provider is used without alarming.
                    if (code is 401 or 403 or 404 or 429)
                        return null;

                    LunaLog.Warning($"ipapi.co returned HTTP {code} for {externalEndpoint.Address}");
                    return null;
                }

                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var output = JsonNode.Parse(body);
                return output?["country"]?.GetValue<string>();
            }
            catch (Exception e)
            {
                LunaLog.Warning($"ipapi.co lookup failed for {externalEndpoint.Address}: {e.Message}");
                return null;
            }
        }
    }
}
