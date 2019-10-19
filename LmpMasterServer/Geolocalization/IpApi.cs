using LmpMasterServer.Geolocalization.Base;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LmpMasterServer.Geolocalization
{
    internal class IpApi : BaseGeolocalization
    {
        public static async Task<string> GetCountry(IPEndPoint externalEndpoint)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    dynamic output = JsonReader.Read(await client.GetStringAsync($"https://ipapi.co/{externalEndpoint.Address}/json/"));
                    return output["country"];
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
