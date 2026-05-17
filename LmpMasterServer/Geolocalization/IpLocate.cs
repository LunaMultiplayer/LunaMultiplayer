using MaxMind.GeoIP2;
using System;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace LmpMasterServer.Geolocalization
{
    internal class IpLocate : IGeolocalization
    {
        public static async Task<string> GetCountryAsync(IPEndPoint externalEndpoint)
        {
            try
            {
                var client = GeolocationHttpClient.GetClient();
                var output = JsonNode.Parse(
                    await client.GetStringAsync($"https://www.iplocate.io/api/lookup/{externalEndpoint.Address}"));
                return output?["country_code"]?.GetValue<string>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<string> GetCountryAsyncGeoIp2Async(IPEndPoint externalEndpoint)
        {
            try
            {
                var licenseKey = Environment.GetEnvironmentVariable("license_key");

                if (!int.TryParse(Environment.GetEnvironmentVariable("account_id"), out var accountId) || string.IsNullOrEmpty(licenseKey))
                    return null;

                var client = new WebServiceClient(accountId, licenseKey);
                var country = await client.CountryAsync(externalEndpoint.Address);
                return country.Country.IsoCode;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
