using LmpMasterServer.Geolocalization.Base;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LmpMasterServer.Geolocalization
{
    internal class IpLocate : BaseGeolocalization
    {
        public static async Task<string> GetCountry(IPEndPoint externalEndpoint)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    dynamic output = JsonReader.Read(await client.GetStringAsync($"https://www.iplocate.io/api/lookup/{externalEndpoint.Address}"));
                    return output["country_code"];
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
